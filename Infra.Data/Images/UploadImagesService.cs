using Domain.Interfaces.Infra.Data;
using Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Formats.Jpeg;
using Image = SixLabors.ImageSharp.Image;

namespace Infra.Data.Images
{
    public class UploadImagesService : IImagesInterface
    {
        private readonly IWebHostEnvironment _environment;
        public UploadImagesService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        //--------------------------------------------------------------------------------------------------------------------------

        //Este método recebe um arquivo de imagem por meio do parâmetro image e verifica se ele é uma imagem.Em seguida, cria um fluxo de memória
        //para a imagem e copia o arquivo para esse fluxo.Define a posição do fluxo como 0 para garantir que a leitura comece no início. 
        //Em seguida, gera um nome de arquivo exclusivo para a imagem com a extensão .jpeg.Utiliza o ambiente da web para acessar o caminho raiz da web e, 
        //com isso, combina a pasta de imagens e o nome do arquivo para formar o caminho completo da imagem. Finalmente, chama o método SalvarImagemAsync 
        //para realizar o processo de salvar a imagem.
        public async Task<(OperationResultModel, string)> UploadImageAsync(IFormFile image)
        {
            if (!IsImage(image))
            {
                return (new OperationResultModel(false, "O arquivo fornecido não é uma imagem válida. Certifique-se de enviar uma imagem nos formatos" +
                    " .jpg, .jpeg, .png ou .gif.\"."), null);              
            }

            var maxSizeInBytes = 5 * 1024 * 1024; 

            if (!IsImageSizeValid(image, maxSizeInBytes))
            {
                return (new OperationResultModel(false, "O tamanho do arquivo excede o limite permitido de 5 MB."), null);
            }

            var ms = new MemoryStream();
            await image.CopyToAsync(ms);
            ms.Position = 0;
            var fileName = $"{Guid.NewGuid()}.jpeg";
            var imagePath = Path.Combine(_environment.WebRootPath, "images", fileName);
            var success = await SaveImage(imagePath, ms, true);

            if (success)
            {
                return (new OperationResultModel(true, "Imagem salva com sucesso."), fileName);

            }

            return (new OperationResultModel(false, "Falha ao salvar a imagem no servidor."), null);            
        }



        //O método SaveImage foi cuidadosamente projetado para lidar com o redimensionamento e o armazenamento de imagens em formato JPEG.
        //Ele inicia carregando a imagem de um fluxo de memória e, em seguida, redimensiona a imagem, se necessário, para um tamanho quadrado para o card.
        //A função também garante que o diretório de imagens exista antes de salvar a imagem e ajusta o caminho de arquivo com a extensão JPEG. 
        //Caso ocorra algum erro durante o processo, o método captura e trata exceções para evitar quebras inesperadas. Por fim, ele retorna
        //um booleano indicando se a imagem foi salva com sucesso ou não. Este código promove a legibilidade, modularidade e manutenção adequada, 
        //seguindo as melhores práticas de manipulação de imagens em um ambiente de aplicativo web.
        private async Task<bool> SaveImage(string imagePath, MemoryStream ms, bool resize = true)
        {
            try
            {               
                var imageLoaded = await Image.LoadAsync(ms);
                var extensionImage = Path.GetExtension(imagePath).ToLower();
                if (resize)
                {
                    var sizeImage = imageLoaded.Size;
                    var ladoMenor = (sizeImage.Height < sizeImage.Width) ? sizeImage.Height : sizeImage.Width;
                    imageLoaded.Mutate(x =>
                    {
                        x.Resize(new ResizeOptions
                        {
                            Size = new Size(ladoMenor, ladoMenor),
                            Mode = ResizeMode.Crop
                        }).BackgroundColor(new Rgba32(255, 255, 255, 0));
                    });
                }
                var webRootPath = _environment.WebRootPath;
                var folderImages = Path.Combine(webRootPath, "images");

                if (!Directory.Exists(folderImages))
                {
                    Directory.CreateDirectory(folderImages);
                }

                var pathComplete = Path.Combine(folderImages, Path.GetFileName(imagePath));
                pathComplete = pathComplete.Replace(extensionImage, ".jpeg");
                imageLoaded.Save(pathComplete, new JpegEncoder());

                return true;
            }
            catch
            {
                return false;
            }
        }


        /// Verifica se o arquivo fornecido é uma imagem com uma das extensões permitidas.
        /// Retorna verdadeiro se o arquivo for uma imagem com uma das extensões .jpg, .jpeg, .png ou .gif.
        /// Caso contrário, retorna falso.
        /// Retorna um valor booleano indicando se o arquivo é uma imagem ou não.
        private bool IsImage(IFormFile file)
        {
            if (file == null) return false;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            return allowedExtensions.Contains(fileExtension);
        }


        //verifica se o tamanho é válido
        private bool IsImageSizeValid(IFormFile file, int maxSizeInBytes)
        {
            if (file == null) return false;

            return file.Length < maxSizeInBytes;
        }



        //-----------------------------------------------------------------------------------------------------
    }
}
