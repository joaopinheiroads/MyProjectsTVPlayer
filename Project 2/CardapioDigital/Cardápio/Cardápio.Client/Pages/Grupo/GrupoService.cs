using Cardápio.Client.Dto;
using Cardápio.Client.Infra.Provider;
using Cardápio.Client.Pages;
using System.Collections.ObjectModel;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Net.Http.Json;

namespace Cardápio.Client.Pages.Grupo
{
    public class GrupoService
    {
        private HttpClient _httpClient;
        private CustomAuthenticationStateProvider _provider;

        public GrupoService(HttpClient httpClient, CustomAuthenticationStateProvider provider)
        {
            _httpClient = httpClient;
            _provider = provider;
        }

        public async Task<List<GrupoGetDTO>> GetData()
        {
            try
            {
                List<GrupoGetDTO> result = await _httpClient.GetFromJsonAsync<List<GrupoGetDTO>>("/api/group/GetGroups");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return null;
            }
        }

        public async Task OnCreateGroupConfirm(GrupoAddDTO grupoDataDTO)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                HttpResponseMessage result = await _httpClient.PostAsJsonAsync("/api/group/CreateGroup", grupoDataDTO);
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public async Task<ObservableCollection<GrupoGetDTO>> SetGroupsDeleted(List<GrupoGetDTO> groups)
        {
            ObservableCollection<GrupoGetDTO> gruposExclude = new ObservableCollection<GrupoGetDTO>();

            foreach (var group in groups)
            {
                var empresasExclude = group.Empresas.Where(e => e.Excluido == true).ToList();

                foreach (var empresa in group.Empresas)
                {
                    Console.WriteLine($"Empresa ID: {empresa.ID}, Excluída: {empresa.Excluido}");
                }

                if (empresasExclude.Any())
                {
                    gruposExclude.Add(new GrupoGetDTO
                    {
                        ID = group.ID,
                        Nome = group.Nome,
                        Empresas = empresasExclude.AsEnumerable(),
                        GrupoTipoID = group.GrupoTipoID,
                        Ativo = group.Ativo,
                        Excluido = group.Excluido,
                    });
                }
            }

            return gruposExclude;
        }

        public async Task<ObservableCollection<GrupoGetDTO>> SetGroupsDemo(List<GrupoGetDTO> groups)
        {
            ObservableCollection<GrupoGetDTO> gruposDemos = new ObservableCollection<GrupoGetDTO>();

            foreach (var group in groups)
            {
                var empresasDemos = group.Empresas.Where(e => e.EmpresaTipoID == 1 && e.Excluido == false).ToList();

                if (empresasDemos.Any())
                {
                    gruposDemos.Add(new GrupoGetDTO
                    {
                        ID = group.ID,
                        Nome = group.Nome,
                        Empresas = empresasDemos.AsEnumerable(),
                        GrupoTipoID = group.GrupoTipoID,
                        Ativo = group.Ativo,
                        Excluido = group.Excluido,
                    });
                }
            }

            return gruposDemos;
        }

        public async Task<ObservableCollection<GrupoGetDTO>> SetGroupsClients(List<GrupoGetDTO> groups)
        {
            ObservableCollection<GrupoGetDTO> gruposClients = new ObservableCollection<GrupoGetDTO>();

            foreach (var group in groups)
            {
                var empresasClients = group.Empresas.Where(e => e.EmpresaTipoID == 2 && e.Excluido == false).ToList();

                if (empresasClients.Any())
                {
                    gruposClients.Add(new GrupoGetDTO
                    {
                        ID = group.ID,
                        Nome = group.Nome,
                        Empresas = empresasClients.AsEnumerable(),
                        GrupoTipoID = group.GrupoTipoID,
                        Ativo = group.Ativo,
                        Excluido = group.Excluido,
                    });
                }
            }

            return gruposClients;
        }

        public async Task OnEditGroupConfirm(GrupoUpdateDTO grupoDataDTO, int grupoID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                await _httpClient.PutAsJsonAsync("/api/group/UpdateGroup/" + grupoID, grupoDataDTO);
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        // Retornar grupo inteiro
        public async Task OnReturnGroupConfirm(int grupoID)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());
            HttpResponseMessage result = await _httpClient.PutAsJsonAsync($"/api/group/ReturnGroup/{grupoID}", new { });
            if (!result.IsSuccessStatusCode)
                throw new Exception(await result.Content.ReadAsStringAsync());
        }

        // Retornar empresa específica dentro do grupo
        public async Task OnReturnEmpresaConfirm(int grupoID, int empresaID)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());
            HttpResponseMessage result = await _httpClient.PutAsJsonAsync(
                $"/api/group/ReturnGroup/{grupoID}?empresaID={empresaID}", new { });
            if (!result.IsSuccessStatusCode)
                throw new Exception(await result.Content.ReadAsStringAsync());
        }

        public async Task OnDeleteGroupConfirm(int grupoID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                HttpResponseMessage result = await _httpClient.DeleteAsync("/api/group/DeleteGroup/" + grupoID);

            }
            catch (Exception err)
            {
                throw err;
            }
        }
    }
}
