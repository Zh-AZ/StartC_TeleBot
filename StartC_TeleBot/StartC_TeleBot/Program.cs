using System;
using Telegram.Bot;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using File = System.IO.File;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Exceptions;

  




static IReplyMarkup GetButtons()
{
    ReplyKeyboardMarkup keyboard = new(new[]
    {
        new KeyboardButton[] {"фото", "2"},
        new KeyboardButton[] {"3", "4"}
    });
    return keyboard;
}





string token = "5714895699:AAFE68PYLiOqioe7YbYZLfJcrdE749FVdMY";


var client = new TelegramBotClient(token);
client.StartReceiving(Update, Error);                                                                
Console.ReadLine();
string saveNameFile = String.Empty;
async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
{
    var message = update.Message;
    string saveNameFile = String.Empty;    // Здесь будет хранится название файла

    if (message.Document != null)        
    {
        Download(message.Document.FileId, message.Document.FileName);    //Скачивание к себе на ПК 
        saveNameFile += $"{message.Document.FileName}\n";                 // Присвоение названия файла к пустой переменной

    }
    if (message.Text != null)
    {
        Console.WriteLine($"{message.Chat.FirstName}    |    {message.Text}");

        if (message.Text.ToLower().Contains("файлы"))              // Показать все сохраненные названия файлов
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, saveNameFile);
        }

        if (message.Text.ToLower().Contains("привет"))                        
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "привет");
            return;
        }
        string imagePath = @"../downloaded.jpg";

        if (message.Text == "фото")
        {
            using (var stream = File.OpenRead(imagePath))
            {
                InputOnlineFile inputOnlineFile = new InputOnlineFile(stream);
                await botClient.SendPhotoAsync(message.Chat.Id, inputOnlineFile);
            }
        }

        if (message.Text.ToLower().Contains("хочу кнопки"))
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, message.Text, replyMarkup: GetButtons());
            return;
        }
    }

    if (message.Photo != null)
    {
        await botClient.SendTextMessageAsync(message.Chat.Id, "Фото у меня");
        var fileId = update.Message.Photo.Last().FileId;
        var fileInfo = await botClient.GetFileAsync(fileId);
        var filePath = fileInfo.FilePath;

        //string destinationFilePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\{message.Photo}";
        string destinationFilePath = @"../downloaded.jpg";
        await using FileStream fileStream = File.OpenWrite(destinationFilePath);
        await botClient.DownloadFileAsync(filePath, fileStream);
        fileStream.Close();

        return;
    }

    if (message.Audio != null)
    {
        await botClient.SendTextMessageAsync(message.Chat.Id, "Аудио получено");
        var fileId = update.Message.Audio.FileId;
        var fileInfo = await botClient.GetFileAsync(fileId);
        var filePath = fileInfo.FilePath;

        string destinationFilePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)}\{message.Audio.FileName}";
        await using FileStream fileStream = File.OpenWrite(destinationFilePath);
        await botClient.DownloadFileAsync(filePath, fileStream);
        fileStream.Close();

        await using Stream stream = File.OpenRead(destinationFilePath);
        await botClient.SendDocumentAsync(message.Chat.Id, new InputOnlineFile(stream, message.Document.FileName));
        return;
    }
    //if (message.Document != null)
    //{
    //    Download(message.Document.FileId, message.Document.FileName);
    //    //string docName = message.Document.FileName;
    //    //string saveNameFile = String.Empty;
    //    //saveNameFile += $"{message.Document.FileName}\n";
    //    await botClient.SendTextMessageAsync(message.Chat.Id, "Документ у меня");
    //    //await botClient.SendTextMessageAsync(message.Chat.Id, saveNameFile);

    //    //await botClient.SendTextMessageAsync(message.Chat.Id, "Пон док у меня");
    //    //var fielId = update.Message.Document.FileId;
    //    //var fileInfo = await botClient.GetFileAsync(fielId);
    //    //var filePath = fileInfo.FilePath;

    //    //string destinationFilePath = @"../downloaded.txt";
    //    //string fileName = String.Format($@"{message.Document.FileName}.txt", Guid.NewGuid());
    //    ////string destinationFilePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\{message.Document.FileName}";
    //    //await using FileStream fileStream = File.OpenWrite(filePath);
    //    //await botClient.DownloadFileAsync(filePath, fileStream);
    //    //fileStream.Close();

    //    // Отдать обратно
    //    //await using Stream stream = File.OpenRead(destinationFilePath);
    //    //await botClient.SendDocumentAsync(message.Chat.Id, new InputOnlineFile(stream, message.Document.FileName.Replace(".jpg", "(edited).jpg")));

    //    //await using Stream stream = File.OpenRead(destinationFilePath);
    //    //await botClient.SendDocumentAsync(message.Chat.Id, new InputOnlineFile(stream, message.Document.FileName));

    //    //return;
    //}
}




static async void Download(string fileId, string path)
{
    var client = new TelegramBotClient("5714895699:AAFE68PYLiOqioe7YbYZLfJcrdE749FVdMY");

    var file = await client.GetFileAsync(fileId);
    FileStream fs = new FileStream("_" + path, FileMode.Create);
    await client.DownloadFileAsync(file.FilePath, fs);
    fs.Close();
    fs.Dispose();
}



Task Error(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
{
    var errorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Error API: {apiRequestException.ErrorCode}\n {apiRequestException.Message}",
        _ => exception.ToString()

    };
    Console.WriteLine(errorMessage);
    return Task.CompletedTask;
}

