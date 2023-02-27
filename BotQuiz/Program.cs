using JFA.Telegram.Console;
using Microsoft.VisualBasic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

var users = new List<User>();
var questions = new List<Question>();

int questionIndex = 0;
var question1 = new Question();


question1.QuestionText = "2+1=?";
question1.Choices = new List<string> { "2", "3", "4" };
question1.AnswerIndex = 3;
questions.Add(question1);




var botManager = new TelegramBotManager();
var bot = botManager.Create("5873307952:AAHqKCrXQ0IswGBGUfGsmRuDvNG4jbwjeC8");

var botDetails = await bot.GetMeAsync();
Console.WriteLine(botDetails.FirstName + " is working...");

botManager.Start(NewMessage);





void NewMessage(Update update)
{
    if (update.Type != UpdateType.Message)
        return;

    var user = CheckUser(update);
    var message = update.Message!.Text;

    switch (user.NextMessage)
    {
        case ENextMessage.Created: Created(user); break;
        case ENextMessage.Name: SaveNameAndSendMenu(user, message); break;
        case ENextMessage.Menu: ChooseMenu(user, message); break;
        case ENextMessage.CheckAnswer: CheckAnswer(user, message); break;
        case ENextMessage.AddQuestion: AddQuestion(user, message); break;
        case ENextMessage.AddOption: AddOption(user, message); break;
        case ENextMessage.AddOption2: AddOption2(user, message); break;
        case ENextMessage.AddOption3: AddOption3(user, message); break;
        case ENextMessage.EnterAnswer: AnswerIndex(user, message); break;
    }


    Console.WriteLine(update.Message!.Text);
}


User CheckUser(Update update)
{
    var chatId = update.Message!.From!.Id;

    User? user = users.FirstOrDefault(u => u.ChatId == chatId);

    if (user == null)
    {
        user = new User();
        user.ChatId = chatId;
        user.Results = new List<string>();
        user.CurrentAddingQuestion = new Question();
        user.CurrentAddingQuestion.Choices = new List<string>();
        users.Add(user);
    }

    return user;
}

void Created(User user)
{
    bot.SendTextMessageAsync(user.ChatId, "Ismingizni kiriting: ");

    user.NextMessage = ENextMessage.Name;
}

void SaveNameAndSendMenu(User user, string message)
{
    user.Name = message;
    user.NextMessage = ENextMessage.Menu;

    SendMenu(user);
}
void SendMenu(User user)
{
    var keyboard = new ReplyKeyboardMarkup(new List<List<KeyboardButton>>()
    {
         new List<KeyboardButton>()
        {
            new KeyboardButton("Start Test")
        },
        new List<KeyboardButton>()
        {
            new KeyboardButton("Add Test")
        },
        new List<KeyboardButton>()
        {
            new KeyboardButton("Show Results")
        }

    });
    keyboard.ResizeKeyboard = true;
    var menuText = "Choose Menu: ";


    bot.SendTextMessageAsync(user.ChatId, menuText, replyMarkup: keyboard);
}
void ChooseMenu(User user, string message)
{
    switch (message)
    {
        case "Start Test": StartTest(user); break;
        case "Add Test": AddTest(user, message); break;
        case "Show Results": ShowResults(user); break;
    }
}


void StartTest(User user)
{

    var keyboard = KeyboardButton(questionIndex);

    string Savol = $"{questions[questionIndex].QuestionText}\n";
    foreach (var item in questions[questionIndex].Choices)
    {
        Savol += $"{item}\n";
    }

    bot.SendTextMessageAsync(user.ChatId, Savol, replyMarkup: keyboard);
    user.NextMessage = ENextMessage.CheckAnswer;
}

void CheckAnswer(User user, string message)
{
    var canswer = Convert.ToInt32(message);
    if (questions[questionIndex++].AnswerIndex == canswer)
    {
        bot.SendTextMessageAsync(user.ChatId, "true");
        user.CorrectAnswer++;
    }
    else
    {
        bot.SendTextMessageAsync(user.ChatId, "False");
    }

    if (questionIndex < questions.Count)
    {
        StartTest(user);
    }
    else
    {
        var result = $"{user.CorrectAnswer} / {questions.Count}";
        user.Results.Add(result);
        bot.SendTextMessageAsync(user.ChatId, "Questions ended..");
        user.NextMessage = ENextMessage.Menu;
        SendMenu(user);
        questionIndex = 0;
        user.CorrectAnswer = 0;
    }

}


void AddTest(User user, string message)
{
    bot.SendTextMessageAsync(user.ChatId, "Enter new question..", replyMarkup: new ReplyKeyboardRemove());
    user.NextMessage = ENextMessage.AddQuestion;
};

void AddQuestion(User user, string message)
{
    var question = new Question();

    question.QuestionText = message;

    user.CurrentAddingQuestion = question;
    user.CurrentAddingQuestion.Choices = new List<string>();

    bot.SendTextMessageAsync(user.ChatId, "Enter 1 Options..");

    user.NextMessage = ENextMessage.AddOption;
}

void AddOption(User user, string message)
{
    user.CurrentAddingQuestion.Choices.Add(message);
    bot.SendTextMessageAsync(user.ChatId, "Enter 2 Option..");

    user.NextMessage = ENextMessage.AddOption2;
}


void AddOption2(User user, string message)
{
    user.CurrentAddingQuestion.Choices.Add(message);
    bot.SendTextMessageAsync(user.ChatId, "Enter 3 Option..");

    user.NextMessage = ENextMessage.AddOption3;
}

void AddOption3(User user, string message)
{
    user.CurrentAddingQuestion.Choices.Add(message);



    user.NextMessage = ENextMessage.EnterAnswer;

    bot.SendTextMessageAsync(user.ChatId, "Send correct answer..");

}


void AnswerIndex(User user, string message)
{
    user.CurrentAddingQuestion.AnswerIndex = Convert.ToInt32(message);
    questions.Add(user.CurrentAddingQuestion);
    bot.SendTextMessageAsync(user.ChatId, "Options added succesfully..");
    user.NextMessage = ENextMessage.Menu;
    SendMenu(user);
}

void ShowResults(User user)
{
    var natija = "";
    foreach (var item in user.Results)
    {
        natija += $"{item}\n";
    }

    bot.SendTextMessageAsync(user.ChatId, $" {user.Name}ning natijalari :\n {natija}");
    SendMenu(user);
}


ReplyKeyboardMarkup KeyboardButton(int questionIndex)
{
    var keyboard = new ReplyKeyboardMarkup(new List<List<KeyboardButton>>()
    {
         new List<KeyboardButton>() { new KeyboardButton(questions[questionIndex].Choices[0]) },
         new List<KeyboardButton>() { new KeyboardButton(questions[questionIndex].Choices[1]) },
         new List<KeyboardButton>() { new KeyboardButton(questions[questionIndex].Choices[2]) },

    });
    keyboard.ResizeKeyboard = true;
    return keyboard;
}
