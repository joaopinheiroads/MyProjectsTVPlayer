using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats;

namespace Cardápio.Domain.AdditionalGroup
{
    public static class RedimensionarImagem
    {
        public static void Redimensionar(string caminhoEntrada, string caminhoSaida, int largura, int altura)
        {
            try
            {
                
                using (var image = Image.Load(caminhoEntrada))
                {
                    
                    image.Mutate(x => x.Resize(largura, altura));
                    
                    
                    var ext = Path.GetExtension(caminhoEntrada).ToLower();
                    if (ext == ".png")
                    {
                        image.SaveAsPng(caminhoSaida);
                    }
                    else
                    {
                        image.SaveAsJpeg(caminhoSaida);
                    }
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RedimensionarImagem] 📚 Stack trace: {ex.StackTrace}");
            }
        }
    }
}