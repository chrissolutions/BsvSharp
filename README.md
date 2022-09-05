# BsvSharp

BsvSharp is a C# library that enables application development for BitcoinSV (BSV) for .NET developers. 

BsvSharp is a fork of [KzBSV](https://github.com/kzbsv/KzBsv) version 0.2.0, another C# library for BSV.  KzBSV is intended for BSV application development in C# but due to its dependency on System.Security.Cryptography,   KzBSV does not  work with Blazor WebAssembly .  The inability of System.Security.Cryptography to work with Blazor WebAssembly defeats KzBSV's ability to compete with other BSV tools that primarily target JavaScript and Web application developers.

BsvSharp resolves this problem by using the BouncyCastle library to provide the cryptographic security algorithms used by BSV.  The version of BouncyCastle was ported from [NBitcoin version 2.0.0](https://github.com/MetacoSA/NBitcoin/tree/v2.0.0.0).  NBitcoin is a .NET version of the BitcoinJ library.  It is quite a mature library but contains the technical debt and constraints associated with the BTC Core protocol.  Fortunately, the BouncyCastle portion is isolated and separated from the technical debt of BTC Core.

Some parts of BsvSharp was derived from the <a href="https://github.com/twostack/dartsv">dartsv</a> library.  The dartsv library by <a href="https://www.twostack.org">TwoStack.org</a>, is an implementation of the BSV library written in the Dart programming language.  Other parts was derived from [MoneyButton BSV JavaScript library](https://github.com/moneybutton/bsvmoneybutton/bsv) originally developed by Ryan X. Charles.

BsvSharp leverages these implementations to produce a C# library that works across all environments and runs on the Web, Desktop and Mobile platforms.

BsvSharp uses the [CafeLib](https://github.com/chrissolutions/CafeLib) library.  CafeLib is a collection of C# libraries used for application development and is now located in its own [GitHub Repository](https://github.com/chrissolutions/BsvSharp).

CafeLib is deployed as set of nuget packages found on [nuget.org](https://www.nuget.org/packages?q=CafeLib).  The current version of the CafeLib libraries is 2.0.0.

### BsvSharp.Api

Accompanying BsvSharp is [BsvSharp.Api](https://github.com/chrissolutions/BsvSharp/tree/main/BsvSharp.Api).
This folder contains wrappers to the following BSV API libraries:

- CoinGecko
- CoinMarketCap
- Paymail
- Mapi
  - MatterPool
  - Taal
    The Taal library requires an API key from Taal.
- MatterCloud
- WhatsOnChain

### Sample

The [Samples](https://github.com/chrissolutions/BsvSharp/tree/main/Samples) directory of the BsvSharp repository contains a sample called BlazorWallet.  BlazorWallet is derived from the [Satolearn](https://satolearn.com) Wallet Workshop sample written in JavaScript using the [MoneyButton's BSV JavaScript library](https://github.com/moneybutton/bsv).  This sample inspired a desire to want to have the ability to write BSV applications in C# and Blazor.  BlazorWallet provides a real-world example of how .NET developers can build a BSV application in C# and to run in Blazor WebAssembly.
