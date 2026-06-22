using Cardápio.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cardápio.Infra.Helpers;
using Cardápio.Domain.Enterprise;

namespace Cardápio.Web.Enterprise
{
    [ApiController]
    [Route("api/enterprise")]
    public class EnterpriseController : ControllerBase
    {
        private readonly EnterpriseService _enterpriseService;

        public EnterpriseController(EnterpriseService enterpriseService)
        {
            _enterpriseService = enterpriseService;
        }

        [Authorize]
        [HttpGet("GetEnterprises")]
        public async Task<ActionResult> GetEnterprises()
        {
            return await HandleServiceResponse(() => _enterpriseService.GetEnterprises(User));
        }

        [Authorize]
        [HttpGet("GetEnterprisesByGrupoID/{groupID}")]
        public async Task<ActionResult> GetEnterprisesByGroupID(int groupID)
        {
            return await HandleServiceResponse(() => _enterpriseService.GetEnterprisesByGroupID(User, groupID));
        }

        [Authorize]
        [HttpGet("GetEnterprisesByUserID/{userID}")]
        public async Task<ActionResult> GetEnterprisesByUserId(int userID)
        {
            return await HandleServiceResponse(() => _enterpriseService.GetEnterprisesByUserID(User, userID));
        }

        [HttpGet("GetEnterpriseByName/{empresaNome}")]
        public async Task<ActionResult> GetEnterpriseByName(string empresaNome)
        {
            return await HandleServiceResponse(() => _enterpriseService.GetEnterpriseByName(empresaNome));
        }

        [Authorize]
        [HttpGet("GetEnterpriseByID/{empresaID}")]
        public async Task<ActionResult> GetEnterpriseByID(int empresaID)
        {
            return await HandleServiceResponse(() => _enterpriseService.GetEnterpriseByID(User, empresaID));
        }

        [HttpPost("SearchCep")]
        public async Task<ActionResult> GetCep(string cep)
        {
            return await HandleServiceResponse(() => _enterpriseService.GetCep(cep));
        }

        [Authorize]
        [HttpPost("SaveCompanyImages")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> AddCompanyImages(
         [FromForm] SaveCompanyImagesRequestDTO request)
            {
                return await HandleServiceResponse(() =>
                    _enterpriseService.AddCompanyImages(
                        User,
                        request.Banner,
                        request.Logo));
        }

        [Authorize]
        [HttpPost("RegisterCompany")]
        public async Task<ActionResult> AddCompany(EmpresaAddDTO empresaAddDTO)
        {
            return await HandleServiceResponse(() => _enterpriseService.AddCompany(User, empresaAddDTO));
        }

        [Authorize]
        [HttpPost("RegisterCompanyInGroupID/{groupID}")]
        public async Task<ActionResult> AddCompanyInGroupID(EmpresaAddDTO empresaAddDTO, int groupID)
        {
            return await HandleServiceResponse(() => _enterpriseService.AddCompanyInGroupID(User, empresaAddDTO, groupID));
        }

        [Authorize]
        [HttpPut("EditCompany/{empresaID}")]
        public async Task<ActionResult> EditCompany(int empresaID, EmpresaUpdateDTO empresaUpdateDTO)
        {
            return await HandleServiceResponse(() => _enterpriseService.EditCompany(User, empresaID, empresaUpdateDTO));
        }

        [Authorize]
        [HttpPatch("EditDateDemo/{empresaID}")]
        public async Task<ActionResult> UpdateDaysDemo(int empresaID, DateDTO dateDTO)
        {
            return await HandleServiceResponse(() => _enterpriseService.EditDayDemo(User, empresaID, dateDTO));
        }

        [Authorize]
        [HttpDelete("DeleteCompany/{empresaID}")]
        public async Task<ActionResult> DeleteCompany(int empresaID)
        {
            return await HandleServiceResponse(() => _enterpriseService.DeleteCompany(User, empresaID));
        }

        [Authorize]
        [HttpPost("VerifyCompanyAlreadyExists")]
        public async Task<ActionResult> VerifyCompanyAlreadyExists(Client.Dto.VerifyCompanyDTO empresa)
        {
            return await HandleServiceResponse(() => _enterpriseService.VerifyCompanyAlreadyExists(User, empresa));
        }

        [HttpGet("TestImageAccess/{imageType}/{fileName}")]
        public async Task<ActionResult> TestImageAccess(string imageType, string fileName)
        {
            try
            {
                // Verificar se o tipo de imagem é válido
                if (imageType != "banner" && imageType != "logo")
                {
                    return BadRequest("Tipo de imagem inválido. Use 'banner' ou 'logo'.");
                }

                // Construir o caminho da imagem
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                var directory = new DirectoryInfo(basePath);
                while (directory != null && !directory.GetFiles("*.csproj").Any())
                {
                    directory = directory.Parent;
                }
                string projectBasePath = directory?.FullName ?? basePath;
                
                string imagePath = Path.Combine(projectBasePath, "imagens", 
                    imageType == "banner" ? "Banner" : "Logo", fileName);


                if (!System.IO.File.Exists(imagePath))
                {
                    return NotFound($"Arquivo não encontrado: {fileName}");
                }

                var fileInfo = new System.IO.FileInfo(imagePath);

                // Verificar se o arquivo não está corrompido
                if (fileInfo.Length == 0)
                {
                    return BadRequest("Arquivo está vazio (0 bytes)");
                }

                // Tentar ler alguns bytes para verificar se é acessível
                try
                {
                    using (var fileStream = System.IO.File.OpenRead(imagePath))
                    {
                        byte[] buffer = new byte[10];
                        int bytesRead = await fileStream.ReadAsync(buffer, 0, 10);
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest($"Erro ao acessar arquivo: {ex.Message}");
                }

                return Ok(new
                {
                    fileName = fileName,
                    imageType = imageType,
                    fullPath = imagePath,
                    exists = true,
                    sizeBytes = fileInfo.Length,
                    lastModified = fileInfo.LastWriteTime,
                    url = $"/imagens/{(imageType == "banner" ? "Banner" : "Logo")}/{fileName}"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TestImageAccess] Erro geral: {ex.Message}");
                return BadRequest($"Erro: {ex.Message}");
            }
        }

        [HttpGet("TestImage/{filename}")]
        public async Task<IActionResult> TestImage(string filename)
        {
            try
            {
                // Testar acesso direto ao arquivo
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                var directory = new DirectoryInfo(basePath);
                while (directory != null && !directory.GetFiles("*.csproj").Any())
                {
                    directory = directory.Parent;
                }
                string projectBasePath = directory?.FullName ?? basePath;
                
                // Procurar nas pastas Banner e Logo
                string[] folders = { "Banner", "Logo" };
                
                foreach (string folder in folders)
                {
                    string imagePath = Path.Combine(projectBasePath, "imagens", folder, filename);
                    Console.WriteLine($"[TestImage] Verificando: {imagePath}");
                    
                    if (System.IO.File.Exists(imagePath))
                    {
                        
                        // Ler e retornar o arquivo
                        byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(imagePath);
                        
                        // Determinar content type
                        string contentType = filename.ToLower().EndsWith(".png") ? "image/png" : 
                                           filename.ToLower().EndsWith(".jpg") || filename.ToLower().EndsWith(".jpeg") ? "image/jpeg" : 
                                           "application/octet-stream";
                        
                        
                        return File(imageBytes, contentType);
                    }
                }
                
                return NotFound($"Imagem {filename} não encontrada");
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro: {ex.Message}");
            }
        }

        private async Task<ActionResult> HandleServiceResponse(Func<Task<ActionResult>> serviceMethod)
        {
            try
            {
                ActionResult result = await serviceMethod();

                if (result is OkObjectResult okResult)
                {
                    return Ok(okResult.Value);
                }

                return Ok(result);
            }
            catch (ErrorResponse ex)
            {

                ErrorResponseDTO errorResponseDto = new ErrorResponseDTO(ex);

                return new ObjectResult(errorResponseDto)
                {
                    StatusCode = ex.CodeError
                };
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException?.Message ?? ex.Message;
                var inner2 = ex.InnerException?.InnerException?.Message ?? "";
                return StatusCode(500, new ErrorResponse($"Erro interno: {innerMsg} | {inner2}", 500));
            }
        }
    }
}
