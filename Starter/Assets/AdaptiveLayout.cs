using GoFish;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdaptiveLayout
{
    private Player _localPlayer;
    private Player _remotePlayer;
    private CardAnimator _cardAnimator;
    private ScreenOrientation _currentScreenOrientation;
    private ScreenOrientation _previousScreenOrientation;

    public AdaptiveLayout(Player localPlayer, Player remotePlayer, CardAnimator cardAnimator)
    {
        _localPlayer = localPlayer;
        _remotePlayer = remotePlayer;
        _cardAnimator = cardAnimator;
    }

    // Update is called once per frame
    public void Update()
    {
        ChangeLayout();
        Debug.Log(_previousScreenOrientation);
        if (_currentScreenOrientation == _previousScreenOrientation)
        {
            return;
        }
        AdjustLayout();
    }

    private void ChangeLayout()
    {
        _previousScreenOrientation = _currentScreenOrientation;
        if (Screen.width > Screen.height)
        {
            _currentScreenOrientation = ScreenOrientation.LandscapeLeft;
        }
        else
        {
            _currentScreenOrientation = ScreenOrientation.Portrait;
        }
    }

    private void AdjustLayout()
    {
        if (Screen.width > Screen.height)
        {
            _localPlayer.ChangeMaxDisplayCards(Constants.PLAYER_LANDSCAPE_SHOWN_CARDS);
            _remotePlayer.ChangeMaxDisplayCards(Constants.BOT_LANDSCAPE_SHOWN_CARDS);
            _currentScreenOrientation = ScreenOrientation.LandscapeLeft;
        }
        else
        {
            _localPlayer.ChangeMaxDisplayCards(Constants.PLAYER_PORTRAIT_SHOWN_CARDS);
            _remotePlayer.ChangeMaxDisplayCards(Constants.BOT_PORTRAIT_SHOWN_CARDS);
            _currentScreenOrientation = ScreenOrientation.Portrait;
        }

        _localPlayer.RepositionDisplayingCards(_cardAnimator);
        _remotePlayer.RepositionDisplayingCards(_cardAnimator);
    }
}
