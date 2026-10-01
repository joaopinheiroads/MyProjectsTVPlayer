using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

public class Verificador : MarshalByRefObject
{
    public string[] Verificar(string pasta)
    {
        var linhas = new List<string>();
        var jaTestadas = new Dictionary<string, string>();

        foreach (var dll in Directory.GetFiles(pasta, "*.dll"))
        {
            Assembly metadados;
            try
            {
                metadados = Assembly.ReflectionOnlyLoadFrom(dll);
            }
            catch
            {
                continue;
            }

            foreach (var referencia in metadados.GetReferencedAssemblies())
            {
                string resultado;
                if (!jaTestadas.TryGetValue(referencia.FullName, out resultado))
                {
                    try
                    {
                        var carregado = Assembly.Load(referencia);
                        var versaoEntregue = carregado.GetName().Version;
                        var origem = carregado.GlobalAssemblyCache ? "GAC" : "bin";
                        resultado = versaoEntregue.Equals(referencia.Version)
                            ? "OK"
                            : "REDIRECIONADO " + versaoEntregue + " " + origem;
                    }
                    catch (Exception erro)
                    {
                        resultado = "FALHA " + erro.GetType().Name;
                    }
                    jaTestadas[referencia.FullName] = resultado;
                }

                if (resultado != "OK")
                    linhas.Add(Path.GetFileName(dll) + " -> " + referencia.Name + " " + referencia.Version + " | " + resultado);
            }
        }

        return linhas.ToArray();
    }

    public string[] Executar(string arquivoTeste)
    {
        var linhas = new List<string>();

        linhas.Add("EXEC RestClient do Apify         : " + Tentar(() =>
        {
            var restSharp = Assembly.Load("RestSharp");
            var tipoOpcoes = restSharp.GetType("RestSharp.RestClientOptions", true);
            var opcoes = Activator.CreateInstance(tipoOpcoes, "https://api.apify.com");
            var tipoCliente = restSharp.GetType("RestSharp.RestClient", true);
            var construtor = tipoCliente.GetConstructors()
                .First(c => c.GetParameters().Length > 0 && c.GetParameters()[0].ParameterType == tipoOpcoes);
            var argumentos = construtor.GetParameters()
                .Select((p, i) => i == 0 ? opcoes : (p.HasDefaultValue ? p.DefaultValue : null))
                .ToArray();
            construtor.Invoke(argumentos);
            return "construido";
        }));

        linhas.Add("EXEC System.Text.Json do RestSharp: " + Tentar(() =>
        {
            var restSharp = Assembly.Load("RestSharp");
            var tipoSerializador = restSharp.GetType("RestSharp.Serializers.Json.SystemTextJsonSerializer", true);
            var serializador = Activator.CreateInstance(tipoSerializador);
            var serializar = tipoSerializador.GetMethods()
                .First(m => m.Name == "Serialize" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(object));
            return "serializou " + serializar.Invoke(serializador, new object[] { new[] { 1, 2, 3 } });
        }));

        linhas.Add("EXEC Newtonsoft via TVPlayerAPI  : " + Tentar(() =>
        {
            var api = Assembly.Load("TVPlayerAPI");
            var tipoHelper = api.GetType("TVPlayerAPI.Helpers.IOHelper", true);
            var gravar = tipoHelper.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .First(m => m.Name == "SerializeJsonToFile");
            if (gravar.IsGenericMethodDefinition)
                gravar = gravar.MakeGenericMethod(typeof(string));
            var valores = new object[] { arquivoTeste, "teste" };
            var argumentos = gravar.GetParameters()
                .Select((p, i) => i < valores.Length ? valores[i] : p.DefaultValue)
                .ToArray();
            gravar.Invoke(null, argumentos);
            return "gravou " + File.ReadAllText(arquivoTeste).Trim();
        }));

        return linhas.ToArray();
    }

    private static string Tentar(Func<string> acao)
    {
        try
        {
            return "OK - " + acao();
        }
        catch (Exception erro)
        {
            var raiz = erro;
            while (raiz is TargetInvocationException && raiz.InnerException != null)
                raiz = raiz.InnerException;
            return "FALHA - " + raiz.GetType().Name + ": " + raiz.Message.Split('\r', '\n')[0];
        }
    }
}

public static class Programa
{
    public static int Main(string[] args)
    {
        var setup = new AppDomainSetup { ApplicationBase = args[0], ConfigurationFile = args[1] };
        var dominio = AppDomain.CreateDomain("verificacao", null, setup);
        var verificador = (Verificador)dominio.CreateInstanceFromAndUnwrap(
            typeof(Verificador).Assembly.Location,
            typeof(Verificador).FullName);

        foreach (var linha in verificador.Verificar(args[0]))
            Console.WriteLine(linha);

        foreach (var linha in verificador.Executar(args[2]))
            Console.WriteLine(linha);

        return 0;
    }
}
