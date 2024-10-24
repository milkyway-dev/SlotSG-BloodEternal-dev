using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using JetBrains.Annotations;

public class SocketModel 
{

    public PlayerData playerData;
    public UIData uIData;

    public InitGameData initGameData;

    public ResultGameData resultGameData;

    public int currentBetIndex=0;

        internal SocketModel(){
        this.playerData= new PlayerData();
        this.uIData= new UIData();
        this.initGameData= new InitGameData();
        this.resultGameData= new ResultGameData();
    }

}



[Serializable]
public class ResultGameData
{
    public List<List<int>> resultSymbols { get; set; }
    public bool isFreeSpin {get; set;}
    public int freeSpinCount { get; set; }
    public double jackpot { get; set; }
}


[Serializable]
public class InitGameData
{
    public List<List<string>> Reel { get; set; }
    // public List<List<int>> Lines { get; set; }
    public List<double> Bets { get; set; }
    public bool canSwitchLines { get; set; }
    public List<int> LinesCount { get; set; }
    public List<List<int>> lineData {get; set;}
}


[Serializable]
public class UIData
{
    public List<Symbol> symbols { get; set; }

}

[Serializable]
public class BetData
{
    public double currentBet;
    public double currentLines;
    public double spins;
    //public double TotalLines;
}

[Serializable]
public class AuthData
{
    public string GameID;
    //public double TotalLines;
}

[Serializable]
public class MessageData
{
    public BetData data;
    public string id;
}

[Serializable]
public class InitData
{
    public AuthData Data;
    public string id;
}

[Serializable]
public class AbtLogo
{
    public string logoSprite { get; set; }
    public string link { get; set; }
}

[Serializable]
public class GameData
{
    public List<List<string>> Reel { get; set; }
    public List<List<int>> Lines { get; set; }
    public List<double> Bets { get; set; }
    public bool canSwitchLines { get; set; }
    public List<int> LinesCount { get; set; }
    public List<int> autoSpin { get; set; }
    public List<List<string>> ResultReel { get; set; }
    public List<int> linesToEmit { get; set; }
    public List<List<string>> symbolsToEmit { get; set; }
    public double WinAmout { get; set; }
    public FreeSpins freeSpins { get; set; }
    public List<string> FinalsymbolsToEmit { get; set; }
    public List<string> FinalResultReel { get; set; }
    public double jackpot { get; set; }
    public bool isBonus { get; set; }
    public double BonusStopIndex { get; set; }
}

[Serializable]
public class FreeSpins
{
    public int count { get; set; }
    public bool isNewAdded { get; set; }
}

[Serializable]
public class Message
{
    public GameData GameData { get; set; }
    public UIData UIData { get; set; }
    public PlayerData PlayerData { get; set; }
    public List<string> BonusData { get; set; }
}

[Serializable]
public class Root
{
    public string id { get; set; }
    public Message message { get; set; }
}

// [Serializable]
// public class UIData
// {
//     public Paylines paylines { get; set; }
//     public List<string> spclSymbolTxt { get; set; }
//     public AbtLogo AbtLogo { get; set; }
//     public string ToULink { get; set; }
//     public string PopLink { get; set; }
// }

[Serializable]
public class Paylines
{
    public List<Symbol> symbols { get; set; }
}

[Serializable]
public class Symbol
{
    public int ID { get; set; }
    public string Name { get; set; }
    public JToken Multiplier { get; set;}
    public object defaultAmount { get; set; }
    public object symbolsCount { get; set; }
    public object increaseValue { get; set; }
    public object description { get; set; }
    public int freeSpin { get; set; }
}

[Serializable]
public class Multiplier
{
    [JsonProperty("5x")]
    public double _5x { get; set; }

    [JsonProperty("4x")]
    public double _4x { get; set; }

    [JsonProperty("3x")]
    public double _3x { get; set; }

    [JsonProperty("2x")]
    public double _2x { get; set; }
}

[Serializable]
public class PlayerData
{
    public double Balance { get; set; }
    public double haveWon { get; set; }
    public double currentWining { get; set; }
}

[Serializable]
public class AuthTokenData
{
    public string cookie;
    public string socketURL;
}
