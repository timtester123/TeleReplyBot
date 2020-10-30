Imports Telegram.Bot
Imports Telegram.Bot.Args
Imports Telegram.Bot.Types.ReplyMarkups

Public Class Form1

    Dim botClient As TelegramBotClient
    Dim conf_File As String


    Dim StartCommands As New List(Of String)
    Dim StartCommandsReplys As New List(Of String)

    Dim BOT_API As String
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.Text = Me.Text & " - v." & Application.ProductVersion

        'Config File
        conf_File = Application.StartupPath & "\TeleBot.conf"
        'HTTP BOT API
        BOT_API = GET_CONF("HTTP BOT API=")

        'Get Commands
        GET_COMMANDS()

        'create Bot Client
        start_BOT()



    End Sub


    Private Sub start_BOT()
        Try
            'Bot API, create Bot Client
            botClient = New TelegramBotClient(BOT_API)
            Dim botResult = botClient.GetMeAsync().Result
            AddHandler botClient.OnMessage, AddressOf Bot_OnMessage_start
            ' AddHandler botClient.OnMessage, AddressOf Bot_OnMessage
            botClient.StartReceiving()
            log("connected")
        Catch ex As Exception
            'write Log
            log("Unable to connect, waiting 10 seconds for a retry.")
            Timer_reconnect.Interval = 10000
            Timer_reconnect.Start()
        End Try
    End Sub

    Public Async Sub Bot_OnMessage_start(ByVal sender As Object, ByVal e As MessageEventArgs)
        If e.Message.Text <> Nothing Then
            'dont reply to old messages
            If e.Message.Date.AddHours(2) > Now.AddMinutes(-1) Then

                'Split message into words
                Dim msg As String = e.Message.Text.Trim.ToLower
                Dim words As String() = msg.Split(" ")

                'commands
                For i As Integer = 0 To StartCommands.Count - 1
                    'Split sub Commands
                    Dim subStartCommands As String() = StartCommands(i).Trim.ToLower.Split("|")
                    'sub commands
                    For s As Integer = 0 To subStartCommands.Count - 1
                        Dim cmd As String = subStartCommands(s)
                        If cmd.Contains(" ") Then
                            'sentence
                            If msg.Contains(cmd) Then
                                Dim reply As String = get_Reply(i)
                                Await SendReplyAsync(reply, e)
                                Exit Sub 'end sub
                            End If
                        Else
                            'words
                            For Each word In words
                                'compare
                                If word = cmd Then
                                    Dim reply As String = get_Reply(i)
                                    Await SendReplyAsync(reply, e)
                                    Exit Sub 'end sub
                                End If
                            Next
                        End If

                    Next
                Next
            End If
        End If
    End Sub


    Function get_Reply(ByVal replyNumber As Integer)
        Dim subStartCommandsReplys As String() = StartCommandsReplys(replyNumber).Trim.Split("|")
        Dim rnd As New Random
        Dim randomReplyNumber As Integer = rnd.Next(0, subStartCommandsReplys.Count)
        Dim reply As String = subStartCommandsReplys(randomReplyNumber)
        Return reply
    End Function

    Private Async Function SendReplyAsync(ByVal reply As String, ByVal e As MessageEventArgs) As Task
        'Replace FirstName
        reply = reply.Replace("%FirstName%", e.Message.From.FirstName)
        'send reply
        Await botClient.SendTextMessageAsync(e.Message.Chat, reply)
        'write Log
        log("Replied to: " & e.Message.From.Username & " - " & reply)
    End Function

    Public Async Sub Bot_OnMessage(ByVal sender As Object, ByVal e As MessageEventArgs)
        'If e.Message.Text <> Nothing Then

        '    'dont reply to old messages
        '    If e.Message.Date > Now.AddMinutes(-1) Then

        '        Dim user As String = e.Message.From.Username.ToLower
        '        'write Log
        '        log("Received message - User: " & user & " - Text: " & e.Message.Text)

        '        If e.Message.Text.ToLower() = "/start" Then
        '            'KeyBoard
        '            Dim ReplyKeyboard As ReplyKeyboardMarkup = New ReplyKeyboardMarkup()
        '            Dim rows = New List(Of KeyboardButton())
        '            Dim cols = New List(Of KeyboardButton)
        '            'commands
        '            For Each command_Name As String In command_Names
        '                cols.Add(New KeyboardButton(command_Name))
        '                rows.Add(cols.ToArray())
        '                cols = New List(Of KeyboardButton)
        '            Next
        '            ReplyKeyboard.Keyboard = rows.ToArray()
        '            'Send Message
        '            Await botClient.SendTextMessageAsync(e.Message.Chat, "Choose:", Types.Enums.ParseMode.Default, False, False, 0, ReplyKeyboard)
        '        Else 'check if command received
        '            For i As Integer = 0 To command_Names.Count - 1
        '                If command_Names(i).ToLower = e.Message.Text.ToLower Then
        '                    Dim errMessage As String = ""
        '                    'start command
        '                    Try
        '                        Process.Start(commands(i), params(i))
        '                        Await botClient.SendTextMessageAsync(e.Message.Chat, "Fired command: " & command_Names(i))
        '                        'write log
        '                        log("Fired command: " & command_Names(i))
        '                        Exit For
        '                    Catch ex As Exception
        '                        errMessage = ex.Message
        '                    End Try
        '                    'error
        '                    If errMessage <> "" Then
        '                        Await botClient.SendTextMessageAsync(e.Message.Chat, "Unable to start process: " & command_Names(i) & " Error: " & errMessage)
        '                        'write log
        '                        log("Unable to start process: " & command_Names(i) & " Error: " & errMessage)
        '                        errMessage = ""
        '                    End If

        '                End If
        '            Next
        '        End If

        '    End If
        'End If
    End Sub


    Private Sub Form1_Clode(sender As Object, e As EventArgs) Handles MyBase.Closing
        Try
            botClient.StopReceiving()
        Catch ex As Exception

        End Try
    End Sub


    Private Sub log(ByVal logtext As String)

        'Time
        logtext = Now & " - " & logtext

        'Insert into Listbox
        ListBox.Invoke(New Action(Sub()
                                      ListBox.Items.Insert(0, logtext)
                                  End Sub))

        Dim logFile As String = Application.StartupPath & "\TeleBot.log"
        My.Computer.FileSystem.WriteAllText(logFile, logtext & vbCrLf, True)
    End Sub


    Public Function GET_CONF(ByVal Name As String) As String
        'Read file
        Dim conf As String = My.Computer.FileSystem.ReadAllText(conf_File)
        'Split lines
        Dim lines As String() = conf.Split(vbCrLf)

        Dim ret As String = ""
        'lines
        For Each line As String In lines
            If line.Replace(vbLf, "").Replace(vbCr, "").ToLower.StartsWith(Name.ToLower) Then
                'value in line
                Dim values As String() = line.Split("=")
                If values.Length = 2 Then
                    'value found
                    ret = values(1)
                    'write Log
                    log("Config found: " & Name & ret)
                    Exit For
                End If
            End If
        Next

        Return ret
    End Function
    Public Sub GET_COMMANDS()
        Dim StartCommands_conf_File As String = Application.StartupPath & "\TeleBot_StartCommands.conf"
        'Read file
        Dim conf As String = My.Computer.FileSystem.ReadAllText(StartCommands_conf_File)
        'Split lines
        Dim lines As String() = conf.Split(vbCrLf)

        'lines
        For Each line As String In lines
            line = line.Replace(vbLf, "").Replace(vbCr, "")
            Dim values As String() = line.Split(";")
            If values.Length = 2 Then
                'Start Command
                StartCommands.Add(values(0))
                'Reply
                StartCommandsReplys.Add(values(1))
                'write Log
                log("StartCommand found: " & line)
            End If
        Next
        '
    End Sub

    Private Sub Timer_reconnect_Tick(sender As Object, e As EventArgs) Handles Timer_reconnect.Tick
        Timer_reconnect.Stop()
        start_BOT()
        '
    End Sub

    Private Sub ListBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox.SelectedIndexChanged

    End Sub
End Class
