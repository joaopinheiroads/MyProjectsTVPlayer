using Cardápio.Client.Pages.Mesas;
using Cardápio.Dto;
using Cardápio.Infra.Data;
using Cardápio.Infra.Helpers;
using Cardápio.Infra.Model;
using Cardápio.Infra.Services;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Security.Claims;
using Microsoft.AspNetCore.Http; // Add this for IHttpContextAccessor
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;


namespace Cardápio.Domain.QrCode
{
    public class QrCodeService
    {
        private readonly CardapioUnitOfWork _cardapioUnitOfWork;
        private readonly AppDbContext _appDbContext;
        private readonly Validator _validator;
        private readonly AppPathsService _appPathsService;
        private readonly IHttpContextAccessor _httpContextAccessor; // Add this field
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        private readonly BaseUrlService _baseUrlService;

        // Caminho fixo do logo da aplicação Escolha.ai
        private readonly string _logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "imagens", "Logo", "logoqrcode.png");

        public QrCodeService(CardapioUnitOfWork cardapioUnitOfWork, AppDbContext appDbContext, Validator validator, AppPathsService appPathsService, IHttpContextAccessor httpContextAccessor, BaseUrlService baseUrlService) // Add this parameter
        {
            _cardapioUnitOfWork = cardapioUnitOfWork;
            _appDbContext = appDbContext;
            _validator = validator;
            _appPathsService = appPathsService;
            _httpContextAccessor = httpContextAccessor; // Assign
            _baseUrlService = baseUrlService;
        }

        public byte[] GenerateQrCode(string content, bool isMesaQrCode)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeGenerator.ECCLevel eccLevel = isMesaQrCode ? QRCodeGenerator.ECCLevel.L : QRCodeGenerator.ECCLevel.Q;

                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, eccLevel))
                {
                    using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                    {
                        byte[] qrCodeBytes = qrCode.GetGraphic(10, System.Drawing.Color.Black, System.Drawing.Color.White, false);

                        // Adiciona o logo da aplicação Escolha.ai automaticamente
                        Console.WriteLine($"[QRCODE DEBUG] Verificando logo em: {_logoPath}");
                        Console.WriteLine($"[QRCODE DEBUG] Arquivo existe: {File.Exists(_logoPath)}");

                        if (File.Exists(_logoPath))
                        {
                            return AddLogoToQrCode(qrCodeBytes, _logoPath);
                        }
                        else
                        {
                            Console.WriteLine($"[QRCODE WARN] Logo não encontrada em {_logoPath}, gerando QR code sem logo");
                        }

                        return qrCodeBytes;
                    }
                }
            }
        }

        private byte[] AddLogoToQrCode(byte[] qrCodeBytes, string logoPath)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(qrCodeBytes))
                using (Bitmap qrBitmap = new Bitmap(ms))
                {
                    // Converter QR code para formato RGB para permitir desenho
                    Bitmap qrBitmapRgb = new Bitmap(qrBitmap.Width, qrBitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    using (Graphics g = Graphics.FromImage(qrBitmapRgb))
                    {
                        g.DrawImage(qrBitmap, 0, 0);
                    }

                    using (Bitmap origLogoBitmap = new Bitmap(logoPath))
                    {
                        // Converter logo para formato RGB também
                        Bitmap logoBitmap = new Bitmap(origLogoBitmap.Width, origLogoBitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        using (Graphics g = Graphics.FromImage(logoBitmap))
                        {
                            g.DrawImage(origLogoBitmap, 0, 0);
                        }

                        try
                        {

                            float transparencia = 0.8f; // 0 = totalmente transparente | 1 = totalmente opaco

                            ColorMatrix matrix = new ColorMatrix();
                            matrix.Matrix33 = transparencia;

                            ImageAttributes attributes = new ImageAttributes();
                            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);


                            int qrSize = qrBitmapRgb.Width;
                            int logoSize = (int)(qrSize * 0.25); // Logo ocupa 25% do QR code
                            int logoX = (qrSize - logoSize) / 2;
                            int logoY = (qrSize - logoSize) / 2;

                            using (Graphics graphics = Graphics.FromImage(qrBitmapRgb))
                            {
                                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                                // Desenhar logo no centro
                                graphics.DrawImage(
                                logoBitmap,
                                new Rectangle(logoX, logoY, logoSize, logoSize),
                                0,
                                0,
                                logoBitmap.Width,
                                logoBitmap.Height,
                                GraphicsUnit.Pixel,
                                attributes
                            );
                                                        }

                            Console.WriteLine($"[QRCODE DEBUG] Logo adicionada com sucesso ao QR code");
                        }
                        finally
                        {
                            logoBitmap.Dispose();
                        }
                    }

                    using (MemoryStream outputMs = new MemoryStream())
                    {
                        qrBitmapRgb.Save(outputMs, System.Drawing.Imaging.ImageFormat.Png);
                        qrBitmapRgb.Dispose();
                        return outputMs.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[QRCODE ERROR] Erro ao adicionar logo ao QR code: {ex.Message}");
                Console.WriteLine($"[QRCODE ERROR] Stack trace: {ex.StackTrace}");
                // Retorna o QR code sem logo se houver erro
                return qrCodeBytes;
            }
        }

        public async Task<string> SaveQrCode(string empresaNome, string? mesaNome = null)
        {

            string formattedEmpresaNome = empresaNome.Replace(" ", "-");
            formattedEmpresaNome = System.Text.RegularExpressions.Regex.Replace(formattedEmpresaNome, "-{2,}", "-");

                // Get host dynamically
                var request = _httpContextAccessor.HttpContext?.Request;
                /*string host = request != null
                    ? $"{request.Scheme}://{request.Host.Value}"
                    : "https://localhost"; // fallback if context is missing */

                string qrCodeContent = $"{_baseUrlService.GetBaseUrl()}loja/{formattedEmpresaNome}";
                
                Console.WriteLine($"[QRCODE DEBUG] Empresa nome formatado: {formattedEmpresaNome}");
                Console.WriteLine($"[QRCODE DEBUG] QR Code content: {qrCodeContent}");


            bool isMesaQrCode = !string.IsNullOrWhiteSpace(mesaNome);

            if (isMesaQrCode)
            {
                string formattedMesaNome = mesaNome.Replace(" ", "-");
                formattedMesaNome = System.Text.RegularExpressions.Regex.Replace(formattedMesaNome, "-{2,}", "-");

                qrCodeContent += $"?mesa={formattedMesaNome}";
            }

            byte[] qrCodeImage = GenerateQrCode(qrCodeContent, isMesaQrCode);

            string uniqueFileName = $"{Guid.NewGuid()}.png";
            string qrCodeDir = _appPathsService.GetQrCodeDirectory();
            string filePath = Path.Combine(qrCodeDir, uniqueFileName);

            Console.WriteLine($"[QRCODE DEBUG] Salvando QR Code em: {filePath}");
            Console.WriteLine($"[QRCODE DEBUG] Diretório QrCode: {qrCodeDir}");
            Console.WriteLine($"[QRCODE DEBUG] Diretório existe: {Directory.Exists(qrCodeDir)}");
            Console.WriteLine("QRCODE MESA: " + qrCodeContent);

            try
            {
                // Garantir que o diretório existe
                Directory.CreateDirectory(qrCodeDir);

                await File.WriteAllBytesAsync(filePath, qrCodeImage);

                Console.WriteLine($"[QRCODE DEBUG] QR Code salvo com sucesso: {uniqueFileName}");
                Console.WriteLine($"[QRCODE DEBUG] Arquivo existe após salvar: {File.Exists(filePath)}");

                return uniqueFileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[QRCODE DEBUG] Erro ao salvar a imagem: {ex.Message}");
                Console.WriteLine($"[QRCODE DEBUG] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<ActionResult> GetQrCodeLayouts(ClaimsPrincipal User, int empresaID)
        {
            int? UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyCategoryIdIsPresent(UserID);

            _validator.VerifyCategoryIdIsPresent(empresaID);

            IEnumerable<QrCodeLayoutGetDTO> qrCodeLayoutGet = await _cardapioUnitOfWork.QrCodeLayoutRepo.GetByEmpresaIDAsync(empresaID);

            return new OkObjectResult(qrCodeLayoutGet);
        }

        public async Task<ActionResult> CreateQrCodeLayout(ClaimsPrincipal user, int empresaID, QrCodeLayoutAddDTO qrCodeLayoutDTO)
        {
            int? UserID = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyCategoryIdIsPresent(UserID);

            await VerifyMesaAlreadyExists(UserID ?? 0, qrCodeLayoutDTO, empresaID);

            QrCodeLayout qrCodeLayout = new()
            {
                Ativo = true,
                Nome = qrCodeLayoutDTO.Nome,
                TextoTitulo = qrCodeLayoutDTO.TitleText,
                TextoDescricao = qrCodeLayoutDTO.DescriptionText,
                CorBorda = qrCodeLayoutDTO.BorderColor,
                CorFundo = qrCodeLayoutDTO.BackgroundColor,
                CorTexto = qrCodeLayoutDTO.TextColor,
                RadioBorda = qrCodeLayoutDTO.BorderRadius,
                DataCadastro = new DateTime(),
                DataEdicao = new DateTime(),
                EmpresaID = empresaID,
            };

            await _cardapioUnitOfWork.QrCodeLayoutRepo.AddAsync(qrCodeLayout);
            await _cardapioUnitOfWork.Commit();

            return new OkResult();
        }

        public async Task VerifyMesaAlreadyExists(int UserID, QrCodeLayoutAddDTO verifyQrCodeDTO, int empresaID)
        {
            IEnumerable<QrCodeLayout> queryQrCodeLayout = _appDbContext.QrCodeLayout.Where(c => c.Nome == verifyQrCodeDTO.Nome
                                                               && c.Ativo
                                                               && c.EmpresaID == empresaID).ToList();

            foreach (QrCodeLayout qrCodeLayout in queryQrCodeLayout)
            {
                _validator.VerifyMesaAlreadyExists(qrCodeLayout);
            }
        }
    }
}
