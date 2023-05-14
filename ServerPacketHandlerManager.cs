using Google.Protobuf;
using SuperMaxim.Messaging;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum ContentType
{
    None = 0,
    GamePlayProfile,
    WeeklyRank,
    DailyBonus,
    GameNotify,
    Shop
}

public class PacketHandler
{
    public Action<PacketInfo> handler;

    public PacketHandler(Action<PacketInfo> handler)
    {
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
    private Dictionary<ContentType, PacketHandler> packetHandlers;

    public ServerContentsManager()
    {
        packetHandlers = new Dictionary<ContentType, PacketHandler>();
        SetHandlers();
    }

    ~ServerContentsManager()
    {
        RemoveHandlers();
    }

    private void SetHandlers()
    {
        AddHandler(ContentType.GamePlayProfile, GamePlayProfileHandler);
        AddHandler(ContentType.WeeklyRank, WeeklyRankHandler);
        AddHandler(ContentType.DailyBonus, DailyBonusHandler);
        AddHandler(ContentType.GameNotify, GameNotifyHandler);
        AddHandler(ContentType.Shop, ShopHandler);
    }

    private void RemoveHandlers()
    {
        RemoveHandler(ContentType.GamePlayProfile, GamePlayProfileHandler);
        RemoveHandler(ContentType.WeeklyRank, WeeklyRankHandler);
        RemoveHandler(ContentType.DailyBonus, DailyBonusHandler);
        RemoveHandler(ContentType.GameNotify, GameNotifyHandler);
        RemoveHandler(ContentType.Shop, ShopHandler);
    }

    public void AddHandler(ContentType contentType, Action<PacketInfo> handler)
    {
        if (packetHandlers.ContainsKey(contentType))
        {
            Debug.LogWarn($"{contentType} already exists.");
            return;
        }

        PacketHandler content = new PacketHandler(handler);
        packetHandlers[contentType].handler();
    }

    public void RemoveHandler(ContentType contentType)
    {
        if (packetHandlers.ContainsKey(contentType))
        {
            packetHandlers.Remove(contentType);
        }
        else
        {
            Debug.LogWarn("Cannot find handler : " + contentType);
        }
    }

    public void HandlePacketContents(ServerPacketToC msg)
    {
        foreach (var info in msg.Infos)
        {
            if(packetHandlers.ContainsKey(info.contentType))
            {
                packetHandlers[info.contentType].handler(info);
            }
        }
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
