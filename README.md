# TradingBot

## Description
This is a simple trading bot that can be connected to a trading account(currently limited to coinbase) and executes trades based on a configured strategy.

## Usage
To run the code locally, you will need to provide access to a postgresql server, and configure the connection string in TradingBot.Worker.Example.AppSettings.json.
You will also need to provide your own api-key and secret for coinspot, as with the above please reference the placeholder values found in the example appsettings.

## Features
- This project allows for easy creation of worker agents that can update the historical data on set intervals.
- A worker that accepts a defined strategy and executes it on a set interval(currently daily)
- A Backtest Worker that accepts a defined strategy and runs it against historical data, outputting the result.

## Future Plans
- Extend the Configurability of strategies, allowing for them to also define an execution frequency
- Extend API to allow triggering of Backtests
- Create a UI to easily visualise performance, configure strategies and execute backtests
- Dockerise project to allow easy deployment (eg. to a raspberry pi)
