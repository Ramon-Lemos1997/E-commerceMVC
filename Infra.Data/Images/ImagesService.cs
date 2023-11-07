using Domain.Interfaces.Infra.Data;
using Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Formats.Jpeg;
using Image = SixLabors.ImageSharp.Image;

namespace Infra.Data.Images
{
    public class ImagesService : IImagesInterface
    {
        private readonly IWebHostEnvironment _environment;
        public ImagesService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        //--------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Faz o upload de uma imagem para o servidor, verificando se ela atende a requisitos específicos.
        /// </summary>
        /// <param name="image">O arquivo de imagem a ser carregado.</param>
        /// <returns>Uma tupla contendo um objeto OperationResultModel indicando o resultado da operação de upload e o nome do arquivo salvo.</returns>
        public async Task<(OperationResultModel, string)> UploadImageAsync(IFormFile image)
        {
            try
            {
                if (!IsImage(image))
                {
                    return (new OperationResultModel(false, "O arquivo fornecido não é uma imagem válida. Certifique-se de enviar uma imagem nos formatos" +
                        " .jpg, .jpeg, .png ou .gif.\"."), null);
                }

                if (!IsImageSizeMin(image))
                {
                    return (new OperationResultModel(false, "A imagem deve ter o tamanho mínimo de 200x200 pixels."), null);
                }


                var maxSizeInBytes = 30 * 1024 * 1024;

                if (!IsImageSizeValid(image, maxSizeInBytes))
                {
                    return (new OperationResultModel(false, "O tamanho do arquivo excede o limite permitido de 30 MB."), null);
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
            catch (Exception ex)
            {
                return (new OperationResultModel(false, $"Exceção não planejada: {ex.Message}"), null);
            }
        }


        //------------------------------------------------------------------------------------------

        /// <summary>
        /// Salva uma imagem no servidor no caminho especificado, com opção de redimensionamento.
        /// </summary>
        /// <param name="imagePath">O caminho onde a imagem será salva.</param>
        /// <param name="ms">O stream da imagem a ser salva.</param>
        /// <param name="resize">Indica se a imagem deve ser redimensionada. O padrão é verdadeiro.</param>
        /// <returns>Verdadeiro se a imagem for salva com sucesso, caso contrário, falso.</returns>
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

        /// <summary>
        /// Verifica se o arquivo fornecido é uma imagem com base em sua extensão de arquivo.
        /// </summary>
        /// <param name="file">O arquivo a ser verificado.</param>
        /// <returns>Verdadeiro se o arquivo for uma imagem nos formatos .jpg, .jpeg, .png ou .gif, caso contrário, falso.</returns>
        private bool IsImage(IFormFile file)
        {
            if (file == null) return false;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            return allowedExtensions.Contains(fileExtension);
        }

        /// <summary>
        /// Verifica se o tamanho do arquivo de imagem está dentro do limite especificado.
        /// </summary>
        /// <param name="file">O arquivo de imagem a ser verificado.</param>
        /// <param name="maxSizeInBytes">O tamanho máximo permitido em bytes.</param>
        /// <returns>Verdadeiro se o tamanho do arquivo de imagem estiver abaixo do limite especificado, caso contrário, falso.</returns>
        private bool IsImageSizeValid(IFormFile file, int maxSizeInBytes)
        {
            if (file == null) return false;

            return file.Length < maxSizeInBytes;
        }

        /// <summary>
        /// Verifica se o tamanho da imagem atende ao requisito mínimo especificado em termos de largura e altura.
        /// </summary>
        /// <param name="image">O arquivo de imagem a ser verificado.</param>
        /// <returns>Verdadeiro se a imagem atender ao tamanho mínimo especificado, caso contrário, falso.</returns>
        private bool IsImageSizeMin(IFormFile image)
        {
            using var img = Image.Load(image.OpenReadStream());
            return img.Width >= 200 && img.Height >= 200;
        }


        //-----------------------------------------------------------------------------------------------------
    }
}
