# Chara.Chat SV
Stardew Valley mod to allow players to chat with NPCs using generative AI.

![Gameplay screenshot 1](repo-media/abigail-rocks.webp)

## For developers

### API Keys (Important!!)

Currently, you need an OpenAI private API key to use this mod. **DO NOT SHARE OR COMMIT THIS INFORMATION.** Anyone you share it with can ultimately use it to cost you money.  [You can get an API key here.](https://openai.com/api/)

By downloading or using this mod, you agree to indemnify the developers of any and all claims arising from its use, including but not limited to costs incurred.

In this project, you can copy apiKeys-dummy.json to a new file, call it apiKeys-secret.json, and paste your API key there. This filename is ignored by git, so it will not be committed unless you mess with the filename or the .gitignore file.

When you make a debug build, **your private key will be copied to the build output directory.** This allows you to begin using it for testing right away. However, this means that anyone who gets a copy of this directory will get your private API key, and can use it to play the game or for any other purpose. Therefore, **DO NOT DISTRIBUTE DEBUG BUILDS.**

Release builds will publish a dummy version of this file; users will need to provide their own key to use it.

![Gameplay screenshot 2](repo-media/kent-tool.webp)

### Roadmap
* Interfacing with other AI providers, including locally-run
* Better support for languages with non-Latin scripts
* Efficiency improvements. Much string manipulation was not handled in a low-garbage way.
* Automated unit tests

Pull requests are welcome. Please adhere to C# coding standards where possible, maintain good architecture, and provide a clear explanation of what your changes do.

![Gameplay screenshot 3](repo-media/alex-leg.webp)

## For users

I recommend you get [this mod from NexusMods.](https://www.nexusmods.com/stardewvalley/mods/16808) You will need [SMAPI](https://smapi.io/) as well.

You will still need an API key, and you should follow the above warnings for it as well. Instructions are on the Nexus page.

![Gameplay screenshot 4](repo-media/clint-copper.webp)