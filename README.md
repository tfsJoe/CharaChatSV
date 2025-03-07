# Chara.Chat SV
Stardew Valley mod to allow players to chat with NPCs using generative AI.

## For developers

### API Keys (Important!!)

Currently, you need an OpenAI private API key to use this mod. **DO NOT SHARE OR COMMIT THIS INFORMATION.** Anyone you share it with can ultimately use it to cost you money.  [You can get an API key here.](https://openai.com/api/)

By downloading or using this mod, you agree to indemnify the developers of any and all claims arising from its use, including but not limited to costs incurred.

In this project, you can copy apiKeys-dummy.json to a new file, call it apiKeys-secret.json, and paste your API key there. This filename is ignored by git, so it will not be committed unless you mess with the filename or the .gitignore file.

When you make a debug build, **your private key will be copied to the build output directory.** This allows you to begin using it for testing right away. However, this means that anyone who gets a copy of this directory will get your private API key, and can use it to play the game or for any other purpose. Therefore, **DO NOT DISTRIBUTE DEBUG BUILDS.**

Release builds will publish a dummy version of this file; users will need to provide their own key to use it.

### Roadmap
* Interfacing with other AI providers, including locally-run
* Better support for languages with non-Latin scripts
* Efficiency improvements. Much string manipulation was not handled in a low-garbage way.
* Automated unit tests

Pull requests are welcome. Please adhere to C# coding standards where possible, maintain good architecture, and provide a clear explanation of what your changes do.