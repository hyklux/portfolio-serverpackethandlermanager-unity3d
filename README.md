# portfolio-serverpackethandlermanager-unity3d
Unity3d 프로젝트에서 ServerPacketHandler를 Chain of Responsibility 디자인 패턴을 이용해 개선한 내용에 대한 설명입니다.

## 기존
- switch문에서 각 패킷Id에 대한 분기를 태우며 각 패킷ID을 처리하는 코드가 작성되어 있었음.
- 라이브 서비스 기간이 늘어날 수록 계속하여 늘어나는 패킷에 의해 함수가 무제한으로 길어짐.
- 코드가 한 곳에 몰려 있어 유지보수 및 디버깅이 어려움.
``` c#
    void HandlePacket(long packetId)
    {
        switch(packetId)
        {
            case GamePlayProfile:
                {
                    //패킷 처리
                }
                break;
            case WeeklyRank:
                {
                    //패킷 처리
                }
                break;
            case DailyBonus:
                {
                    //패킷 처리
                }
                break;
            case GameNotify:
                {
                    //패킷 처리
                }
                break;
            case Shop:
                {
                    //패킷 처리
                }
                break;

            //... 패킷이 늘어날 수록 함수의 내용이 무한정 늘어남.
        }
    }
```

## 개선
- 각 패킷Id에 대한 처리를 담당하는 Handler 함수를 생성하고 각 패킷 응답시 처리할 내용을 작성.
``` c#
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
```
- 상시 처리되어야 할 패킷 핸들러는 게임 첫 실행 시 등록하고 사용하고, 특정 씬이나 팝업에서 처리되는 것들은 해당씬 매니저나 팝업 스크립트에서 등록
``` c#
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
```
- 패킷Id가 오면 그에 매칭되는 Handler함수에서 필요한 처리를 수행(Chain of Responsibility 디자인 패턴 적용)
``` c#
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
```
- 각 패킷별로 처리할 내용을 개별 클래스로 분리하여 코드를 decoupling 시킴.
- 유지보수 및 디버깅이 용이해짐.
