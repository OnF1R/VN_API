namespace VN_API.Extensions
{
    public class ImageConverter
    {
        public static byte[] ConvertImageToByteArray(string imagePath)
        {
            try
            {
                // Чтение изображения из файла
                using (FileStream fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader binaryReader = new BinaryReader(fileStream))
                    {
                        // Чтение файла в виде массива байтов
                        return binaryReader.ReadBytes((int)fileStream.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при конвертации изображения в массив байтов: {ex.Message}");
                return null;
            }
        }
    }
}
