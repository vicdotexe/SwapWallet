# SwapWallet
EVM Wallet with DEX Swap

## Introduction
My goal was to design a wallet application that encompassed any EVM compatible chains, with it's main feature being able to directly make DEX swaps (or trades) from within the wallet application itself rather than having to connect it externally to a decentralized exchange website. At first I was manually collecting different DEX router contracts on different chains and just interracting with them using the NEthereum library's web3 utility. Eventually I came across a service (Li.Fi) that had launched proxy contracts on many chains and an API to streamline what I was manually doing, so I switched over to using that in order to take a lot of the leg work out of calculations and finding valid router contracts.

## Notes
- This project was my way of introducing myself to both Xamarin.Forms and the MVVM pattern, as well as a little web3 exploration. 
- Like most my projects, it was never intended to be a complete functioning application. 
- It is about 6 months old at the time of writing this readme, and after a leave of absence, some features are breaking due to either a change in an API I'm using, or possibly the recent ethereum merge: -> <span style="color:red"><b><i>Attempting to import popular tokens on certain chains will either crash the app or just not work. To avoid this, you should only play with the known working chains on the swap page: BSC and Fantom.</b></i></span>
- Functionality for importing tokens on the assets page was lost at some point.

## Details

Here's a demonstration video:
[![image](https://user-images.githubusercontent.com/25494980/214176623-75e586a4-618e-433a-8a20-b454dc9c329f.png)](https://watch.screencastify.com/v/9qryCfRkhBFUSYsRD2w5)


### Login/User-Creation
You open up the application and are presented with a log in screen where you can create a user. You provide a name and password, and are given a 12 word seed phrase. This will generate a 'User'... A user can have multiple nested 'accounts' (or wallet addresses) created (or imported), which can be done from the Accounts screen once you've logged in. Your seed phrase and private keys will be encrypted with your password and the cipher is stored locally. Users are assigned a uniquely generated avatar associated with their account.

### Navigation
At the top of the flyout is an area to select/visualize which sub-account is currently active on the logged in user, and displays an avatar that was uniquely generated based off the sub-account's unique public address.

### Accounts
Here you can create new sub-accounts, or import existing addresses as sub-accounts. Clicking on any existing account will reveal it's avatar, QR code, public address, the option to retrieve the private key using your password, and the option to alter the display name for that sub-account.

### Assets
The assets page contains a button at the top to choose which chain you want to view the assets of (Ethereum, Binance-Smart-Chain, Polygon, Avalanche, etc.). The first time the assets page loads on a specific sub-account (wallet address), it will attempt to scan through a list of known token addresses to find any balances you already possess. You also optionally have the option to import the address of a lesser known asset. Your assets are displayed in a list featuring the logo, ticker symbol, price, your current balance, and the USD value of your balance.

### Swap
This has the typical layout of a DEX swap interface, with the granularity bonus of being able to choose a chain and combination of exchanges/aggregators to find the best deal. You select which token you want to swap 'from' and 'to', enter an desired amount to swap from, and it will calculate the estimated amount of the 'to' token that you will recieve. Slippage will set how much of a variance you're willing to accept should the estimation fall short due to fees or changes in the liquidity pool by the time your transaction goes through.
