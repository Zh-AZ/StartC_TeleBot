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

static void CreateDirectory()                        //Создание директория для хранения на диске отправленных файлов             
{
    string path = @"..\net6.0";
    string subpath = @"downloaded";
    if (!Directory.Exists(path))
        Directory.CreateDirectory(path);
    
    Directory.CreateDirectory($"{path}/{subpath}");
}


static IReplyMarkup GetButtons()                        
{
    ReplyKeyboardMarkup keyboard = new(new[]
    {
        new KeyboardButton[] { "файлы", "мне скучно" },
        new KeyboardButton[] { "/start", "/help" }
    });
    return keyboard;
}


string token = "5714895699:AAFE68PYLiOqioe7YbYZLfJcrdE749FVdMY";


var client = new TelegramBotClient(token);
client.StartReceiving(Update, Error);                                                                
Console.ReadLine();

async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
{
    var message = update.Message;
    string path = @"..\net6.0\downloaded\";
    
    if (message.Document != null)        
    {
        CreateDirectory();
        Download(message.Document.FileId, message.Document.FileName);
        Console.WriteLine($"{message.Chat.FirstName}    |    {message.Document.FileName} - {message.Document.FileSize}(B)");
        await botClient.SendTextMessageAsync(message.Chat.Id, "Документ сохранен");
    }

    if (message.Photo != null)
    {
        CreateDirectory();
        var guidName = $"{DateTime.Now.Ticks.GetHashCode().ToString("x").ToUpper()}.jpg";
        Download(message.Photo[message.Photo.Length - 1].FileId, guidName);
        Console.WriteLine($"{message.Chat.FirstName}    |    {guidName} - {message.Photo[message.Photo.Length - 1].FileSize}(B)");
        await botClient.SendTextMessageAsync(message.Chat.Id, "Изображение сохранено");
    }

    if (message.Sticker != null)
    {
        await botClient.SendStickerAsync(message.Chat.Id, message.Sticker.FileId);
        Console.WriteLine($"{message.Chat.FirstName}    |    {message.Sticker} - {message.Sticker.FileSize}(B)");
    }
    
    if (message.Text != null)
    {
        Console.WriteLine($"{message.Chat.FirstName}    |    {message.Text}");
        
        if (message.Text.StartsWith("/start") || message.Text.ToLower().Contains("привет"))
        {
            await botClient.SendStickerAsync(message.Chat.Id, "https://cdn.tlgrm.app/stickers/0d6/cda/0d6cda3a-19ed-4b2f-81b2-ec8fd9005df0/192/5.webp");
            await botClient.SendTextMessageAsync(message.Chat.Id, "Команда /help для мини-справки");
        }
        
        else if (message.Text.StartsWith("/help"))
        {
            string help = "Я могу принимать изображение, документы, видео и аудиосообщения\nДля использования кнопок надо написать <хочу кнопки>\n" +
                          "Чтобы посмотеть список сохраненных файлов - <файлы>";
            await botClient.SendTextMessageAsync(message.Chat.Id, help);
        }

        else if (message.Text.ToLower().Contains("мне скучно"))
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "https://slowroads.io/");
        }
        
        else if (message.Text.ToLower().Contains("файлы"))
        {
            if (Directory.Exists(path))
            {
                string onlyName = OnlyFileName(path);
                await botClient.SendTextMessageAsync(message.Chat.Id, onlyName);
            }

            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Ничего не знаю");
            }
        }

        else if (message.Text.ToLower().Contains("хочу кнопки"))
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, message.Text, replyMarkup: GetButtons(), replyToMessageId: message.MessageId);
            return;
        }

        else
        {
            await botClient.SendStickerAsync(message.Chat.Id, "https://cdn.tlgrm.app/stickers/ccd/a8d/ccda8d5d-d492-4393-8bb7-e33f77c24907/192/12.webp");
        }

        if (Directory.Exists(path))
        {
            string onlyName = OnlyFileName(path);
            string[] nameFiles = onlyName.Split('\n');
            for (int i = 0; i < nameFiles.Length; i++)
            {
                if (message.Text == nameFiles[i])
                {
                    string filePath = $@"{@"..\net6.0\downloaded"}\{nameFiles[i]}";
                    await using Stream stream = File.OpenRead(filePath);
                    await botClient.SendDocumentAsync(message.Chat.Id, new InputOnlineFile(stream, nameFiles[i]));
                }
            }
        }
    }

    if (message.Audio != null)
    {
        CreateDirectory();
        await botClient.SendTextMessageAsync(message.Chat.Id, "Аудио сохранено");
        Download(message.Audio.FileId, message.Audio.FileName);
        Console.WriteLine($"{message.Chat.FirstName}    |    {message.Audio.FileName} - {message.Audio.FileSize}(B)");
    }

    if (message.Video != null)
    {
        CreateDirectory();
        var guidName = $"{DateTime.Now.Ticks.GetHashCode().ToString("x").ToUpper()}.mp4";
        Download(message.Video.FileId, guidName);
        Console.WriteLine($"{message.Chat.FirstName}    |    {guidName} - {message.Video.FileSize}(B)");
        await botClient.SendTextMessageAsync(message.Chat.Id, "Видео сохранено");
    }
}


static string OnlyFileName(string path)                    
{
    string[] fileName = Directory.GetFiles(path);        // Сохранение в массив название путей к файлу
    string onlyName = "";
    foreach (string file in fileName)
    {
        onlyName += $"{Path.GetFileName(file)}\n";      // Оставить только название файла с расширением и убрать все лишнее,
    }                                                  //  для более удобного чтения при показе списка сохраненных файлов
    return onlyName;
}


static async void Download(string fileId, string nameFile)
{
    var client = new TelegramBotClient("5714895699:AAFE68PYLiOqioe7YbYZLfJcrdE749FVdMY");

    var file = await client.GetFileAsync(fileId);
    string filePath = $@"{@"..\net6.0\downloaded"}\{nameFile}";
    await using FileStream fileStream = File.OpenWrite(filePath);
    await client.DownloadFileAsync(file.FilePath, fileStream);
    fileStream.Close();
    fileStream.Dispose();
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

