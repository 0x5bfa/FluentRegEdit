<p align="center">
  <img width="128" align="center" src="https://user-images.githubusercontent.com/62196528/206452068-ba9f900d-28f2-4415-b9a4-339515c1282a.png" />
</p>
<h1 align="center">
  Registry Valley
</h1>
<p align="center">
  A powerful yet fluent registry editor
</p>

<p align="center">
  <a title="Platform" target="_blank">
    <img src="https://img.shields.io/badge/Platform-Windows-red" alt="Platform" />
  </a>
</p>

---

## 🎁 Installation

### Building from source

See the [build section](#-building-the-code).

## 📸 Screenshots

![image](https://user-images.githubusercontent.com/62196528/206452528-0e36f5e3-187f-4085-9413-1b8caf7a78ac.png)

## 🧑‍💻 Contributing

There are multiple ways to participate in the community:

- [Submit bugs and feature requests](https://github.com/onein528/RegistryValley/issues/new/choose).
- Make pull requests for anything from typos to additional and new idea
- Review source code changes

### 🏗️ Codebase Structure

```
.
├──src                                    // The source code.
   ├──RegistryValley.App                  // Code for most front-end elements of the app.
   ├──RegistryValley.Core                 // Core elements of the app.
   ├──RegistryValley.Console              // Console playgrounds for Vanara
```

### 🗃️ Contributors

<a href="https://github.com/onein528/RegistryValley/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=onein528/RegistryValley" />
</a>

## 🦜 Feedback

- [Request a new feature](https://github.com/onein528/RegistryValley/pulls)
- Upvote popular feature requests
- [File an issue](https://github.com/onein528/RegistryValley/issues/new/choose)

## 🔨 Building the Code

### 1. Prerequisites

Ensure you have installed the following tools:

- Windows 10 2004 (10.0.19041.0) or later with Developer Mode on in the Windows Settings
- [Git](https://git-scm.com/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) with following individual components:
  - Windows 11 (10.0.22000.0) SDK
  - Windows App SDK
  - .NET 6 SDK

### 2. Git

Clone the repository:

```git
git clone https://github.com/onein528/RegistryValley
```

### 3. Build the project

- Open `RegistryValley.sln`.
- Hit 'Set as Startup item' on `RegistryValley.App` in the Solution Explorer.
- Build with `DEBUG`, `x64`, `RegistryValley.App`.

## 💳 Credit

- Many thanks to [Zee-Al-Eid Ahmad Rana @zeealeid](https://twitter.com/zeealeid) for creating this app's logo.

## 📱 Contact
If you would like to ask a question, please reach out to us via Twitter:

- Tomoyuki Terashita, Main Developer: [@onein528](https://twitter.com/onein528)

## ⚖️ License

Copyright (c) 2022 onein528

Licensed under the MIT license as stated in the [LICENSE](LICENSE).
