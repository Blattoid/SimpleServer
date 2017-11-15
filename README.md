# SimpleServer

This server uses a simple TCP Listener to interact over a network or Internet. (There is no encryption however so be careful that nothing secret is sent like passwords.) Once connected, it optionally asks for a password or just brings you to the menu. This option is in the config file. 
Surveillance is a prominent feature of the server. Incorrect password attempts, commands used and sometimes what the command sent back are displayed on the server console.
If you cannot compile the program yourself or you don’t want to, pre-compiled executables for Windows can be found in the Executables folder. 
If you want to run the server on Linux, use something like Mono, which allows you to execute .NET framework applications on UNIX.
I recommend using something like NetCat to communicate with the server. I did try it with Telnet but it didn't seem to work.


## Commands Documentation:

Here lies the help and documentation for each server command.

### HELP
Simply lists the commands that the server recognises. This is a hard-coded string for the same reason that it isn't in the config file: because you can't alter what commands the server recognises.

### 8BALL
If you have ever owned a __'Magic 8 Ball'__, you know that you ask it a question, shake it, turn it over and it gives you an answer. I implemented this in the early stages of development, just to let you be able to do something when you connected to the server. When you first enter the command, it will greet you and prompt you for a question. Once you enter something (other than nothing), it will spit out an answer to your question. When you are done asking questions, simply enter '__exit__' as a question to quit. Since it is being monitored, the admin may get a good chuckle out of this...

### CAPSLOCKTEXT
If you have ever wanted to make every other character in a sentence capitalised, this is for you. Normally this would take you quite a while to do, since you may often make mistakes and its confusing. Computers can process text like this billions of times faster than a human can. __(Ciation needed)__ Besides, I wanted the server to have more functionality and *usefulness*.
For example, if you were to enter this, it would end up looking like this. --> fOr eXaMpLe, If yOu wErE To eNtEr tHiS, iT WoUlD EnD Up lOoKiNg lIkE ThIs.

### DRIVES
This command was created when I wanted to be able to view more useful information about the server, rather than just asking questions like 'will I have a decent job?'. This command spits out information about volumes attached to the server, like drive letters (if supported), volume labels, free space percentages, along with total capacity on the drive. This is enough information to calculate the total used space on the volume, so I didn't include that. The command doesn't accept any additional input. When the client executes the command, the results are also displayed on the server side, so the admin can see what they see.

### SOCKET
This command gets the IP of both the server and the client, along with the port the server is using, and return it. This is helpful if you are using domains and you want to see the IP of the server. Whilst this is also returned to the server console, this probably doesn’t reveal anything new, since the client IP is logged upon connection, and both port and server IP are probably already known to the admin anyway.

### CREDITS
This is very simple; when run it returns a hard-coded string which gives credit to me for making the program. It also includes a link to my GitHub page, if the user wishes to see more.

### EXIT
This command probably doesn’t need explaining but here goes anyway. When run this command will give a goodbye message (which is configurable in the config file) and close the connection with the user. 
