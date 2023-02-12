# Stardew Chatter

## For developers

### API Keys (Important!!)

Currently, you need an OpenAI private API key to use this mod. **DO NOT COMMIT THIS INFORMATION.** Anyone you share it with can ultimately use it to cost you money.

At this writing, [OpenAI is offering $18 of free credits for 3 months here.](https://openai.com/api/)

In this project, you can copy apiKeys-dummy.json to a new file, call it apiKeys-secret.json, and paste your API key there. This filename is ignored by git, so it will not be committed unless you mess with the filename or the .gitignore file.

When you make a debug build, **your private key will be copied to the build output directory.** This allows you to begin using it for testing right away. However, this means that anyone who gets a copy of this directory will get your private API key, and can use it to play the game or for any other purpose. Therefore, **DO NOT DISTRIBUTE DEBUG BUILDS.**

Release builds will publish a dummy version of this file; users will need to provide their own key to use it.
