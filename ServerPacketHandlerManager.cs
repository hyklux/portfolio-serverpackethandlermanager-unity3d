using Google.Protobuf;
using SuperMaxim.Messaging;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PacketContent
{
    public string id;
    public Action<PacketInfo> handler;

    public PacketContent(string packetId, Action<PacketInfo> handler)
    {
        id = packetId;
        this.handler = handler;
    }
}

/**서버 패킷 응답에 대한 처리를 관리 (Chain of Reaction 디자인 패턴)
상시 처리되어야 할 핸들러는 게임 첫 실행 시 등록하고 사용하고, 특정 씬이나 팝업에서 처리되는 것들은 해당씬 매니저나 팝업 스크립트에 핸드러를 등록하고 해제하는 방식으로 처리
하나의 스크립트 if/switch문으로 여러개의 프로토콜을 분기처리하지 않고 필요한 곳에 개별 프로토콜에 대한 핸드러를 등록/해제하는 방식
서버 패킷 응답에 대한 처리를 훨씬 더 효율적이고 유연하게 처리 가능
*/
public class ServerPacketHandlerManager : Singleton<ServerPacketHandlerManager>
{
    private List<PacketContent> packetHandlers;
    private List<PacketContent> waitHandler;
    private List<PacketContent> removeHandler;

    private bool locked = false;

    public ServerContentsManager()
    {
        packetHandlers = new List<PacketContent>();
        waitHandler = new List<PacketContent>();
        removeHandler = new List<PacketContent>();
        SetHandlers();
    }

    ~ServerContentsManager()
    {
        RemoveHandlers();
        waitHandler.Clear();
        removeHandler.Clear();
        packetHandlers.Clear();
    }

    private void SetHandlers()
    {
        AddHandler("GamePlayProfile", GamePlayProfileHandler);
        AddHandler("WeeklyRank", WeeklyRankHandler);
        AddHandler("DailyBonus", DailyBonusHandler);
        AddHandler("GameNotify", GameNotifyHandler);
        AddHandler("Shop", ShopHandler);
    }

    private void RemoveHandlers()
    {
        RemoveHandler("GamePlayProfile", GamePlayProfileHandler);
        RemoveHandler("WeeklyRank", WeeklyRankHandler);
        RemoveHandler("DailyBonus", DailyBonusHandler);
        RemoveHandler("GameNotify", GameNotifyHandler);
        RemoveHandler("Shop", ShopHandler);
    }

    private int ContainsHandler(string packetId, Action<PacketInfo> handler)
    {
        return packetHandlers.FindIndex(c => c.id == packetId && c.handler == handler);
    }

    public void AddHandler(string packetId, Action<PacketInfo> handler)
    {
        int index = ContainHandler(packetId, handler);
        if (index >= 0)
        {
            Debug.LogWarn($"{packetId} | {handler} already exists.");
            return;
        }

        PacketContent content = new PacketContent(packetId, handler);
        if (locked)
        {
            waitHandler.Add(content);
        }
        else
        {
            packetHandlers.Add(content);
        }
    }

    private void CheckHandler()
    {
        if (removeHandler.Count > 0)
        {
            foreach (var h in removeHandler)
            {
                packetHandlers.Remove(h);
            }

            removeHandler.Clear();
        }

        if (waitHandler.Count > 0)
        {
            foreach (var h in waitHandler)
            {
                packetHandlers.Add(h);
            }

            waitHandler.Clear();
        }
    }

    public void RemoveHandler(string packetId, Action<PacketInfo> handler)
    {
        int index = ContainHandler(packetId, handler);

        if (index >= 0)
        {
            if (locked)
            {
                ServerContent content = new ServerContent(packetId, handler);
                removeHandler.Add(content);
            }
            else
            {
                packetHandlers.RemoveAt(index);
            }
        }
        else
        {
            Debug.LogWarn("Cannot find Handler : " + packetId + "|" + handler);
        }
    }

    public void HandlePacketContents(ServerPacketToC msg)
    {
        locked = true;
        foreach (var info in msg.Infos)
        {
            foreach (var h in packetHandlers)
            {
                if (h.id == info.Id)
                {
                    h.handler(info);
                }
            }
        }
        locked = false;

        CheckHandler();
    }

    #region Event Handler
    public void GamePlayProfileHandler(PacketInfo info)
    {
        if (info.RetCode != ResultCode.Success)
        {
            Debug.LogWarning("GamePlayProfile.resultCode=", info.RetCode);
            return;
        }

        //유저 프로필 업데이트 처리
    }

    public void WeeklyRankHandler(PacketInfo info)
    {
        if (info.Type != "info")
        {
            return;
        }

        if (info.RetCode != ResultCode.Success)
        {
            if (info.RetCode != 3003)
            {
                Debug.LogWarning("WeeklyRank.resultCode=", info.RetCode);
            }
            return;
        }

        //주간 랭킹 처리
    }

    public void DailyBonusHandler(PacketInfo info)
    {
        if (info.Type != "info")
        {
            return;
        }

        if (info.RetCode != ResultCode.Success)
        {
            Debug.LogError("DailyBonus.resultCode=", info.RetCode);
            return;
        }

        //일일 출석 보상 처리
    }

    public void GameNotifyHandler(PacketInfo info)
    {
        if (info.RetCode != ResultCode.Success)
        {
            Debug.LogError("GameNotify.resultCode=", info.RetCode);
            return;
        }

        //노티 처리
    }
    #endregion
}
