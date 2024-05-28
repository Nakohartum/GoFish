using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace GoFish
{
    /// <summary>
    /// Manages the positions of the player's cards
    /// </summary>
    [Serializable]
    public class Player : IEquatable<Player>
    {
        public string PlayerId;
        public string PlayerName;
        public bool IsAI;
        public Transform[] Positions;
        public Vector2 BookPosition;
        private float _cardsOffset;

        int NumberOfDisplayingCards;
        int NumberOfBooks;
        public int MaximumOfDisplayingCardsPerRow;

        public Player()
        {
            
        }

        public Player(Transform[] positions, float cardsOffset)
        {
            Positions = positions;
            _cardsOffset = cardsOffset;
        }

        public List<Card> DisplayingCards = new List<Card>();

        public Vector2 NextCardPosition()
        {
            Vector2 nextPos = Vector2.zero;
            if (NumberOfDisplayingCards > MaximumOfDisplayingCardsPerRow)
            {
                nextPos = Positions[1].position + Vector3.right * _cardsOffset * (NumberOfDisplayingCards % MaximumOfDisplayingCardsPerRow);
                
            }
            else
            {
                nextPos = Positions[0].position + Vector3.right * _cardsOffset * NumberOfDisplayingCards;
            }
            return nextPos;
        }

        public void ChangeMaxDisplayCards(int showCardsAmount)
        {
            MaximumOfDisplayingCardsPerRow = showCardsAmount;
        }


        public Vector2 NextBookPosition()
        {
            Vector2 nextPos = BookPosition + Vector2.right * Constants.PLAYER_BOOK_POSITION_OFFSET * NumberOfBooks;
            return nextPos;
        }

        public void SetCardValues(List<byte> values)
        {
            if (DisplayingCards.Count != values.Count)
            {
                Debug.LogError($"Displaying cards count {DisplayingCards.Count} is not equal to card values count {values.Count} for {PlayerId}");
                return;
            }

            for (int index = 0; index < values.Count; index++)
            {
                Card card = DisplayingCards[index];
                card.SetCardValue(values[index]);
                card.SetDisplayingOrder(index);
            }
        }

        public void HideCardValues()
        {
            foreach (Card card in DisplayingCards)
            {
                card.SetFaceUp(false);
            }
        }

        public void ShowCardValues()
        {
            foreach (Card card in DisplayingCards)
            {
                card.SetFaceUp(true);
            }
        }

        public void ReceiveDisplayingCard(Card card)
        {
            DisplayingCards.Add(card);
            card.OwnerId = PlayerId;
            NumberOfDisplayingCards++;
        }

        public void ReceiveBook(Ranks rank, CardAnimator cardAnimator)
        {
            Vector2 targetPosition = NextBookPosition();
            List<Card> displayingCardsToRemove = new List<Card>();

            foreach (Card card in DisplayingCards)
            {
                if (card.Rank == rank)
                {
                    card.SetFaceUp(true);
                    float randomRotation = UnityEngine.Random.Range(-1 * Constants.BOOK_MAX_RANDOM_ROTATION, Constants.BOOK_MAX_RANDOM_ROTATION);
                    cardAnimator.AddCardAnimation(card, targetPosition, Quaternion.Euler(Vector3.forward * randomRotation));
                    displayingCardsToRemove.Add(card);
                }
            }

            DisplayingCards.RemoveAll(card => displayingCardsToRemove.Contains(card));
            RepositionDisplayingCards(cardAnimator);
            NumberOfBooks++;
        }

        public void RepositionDisplayingCards(CardAnimator cardAnimator)
        {
            PutCards();
            NumberOfDisplayingCards = 0;
            foreach (Card card in DisplayingCards)
            {
                var parent = card.transform.parent;
                if (parent.TryGetComponent(typeof(Card), out var c))
                {
                    continue;
                }
                NumberOfDisplayingCards++;
                cardAnimator.AddCardAnimation(card, NextCardPosition());
            }
        }

        public void SendDisplayingCardToPlayer(Player receivingPlayer, CardAnimator cardAnimator, List<byte> cardValues, bool isLocalPlayer)
        {
            int playerDisplayingCardsCount = DisplayingCards.Count;

            if (playerDisplayingCardsCount < cardValues.Count)
            {
                Debug.LogError("Not enough displaying cards");
                return;
            }

            for (int index = 0; index < cardValues.Count; index++)
            {

                Card card = null;
                byte cardValue = cardValues[index];

                if (isLocalPlayer)
                {
                    foreach (Card c in DisplayingCards)
                    {
                        if (c.Rank == Card.GetRank(cardValue) && c.Suit == Card.GetSuit(cardValue))
                        {
                            card = c;
                            break;
                        }
                    }
                }
                else
                {
                    card = DisplayingCards[playerDisplayingCardsCount - 1 - index];
                    card.SetCardValue(cardValue);
                    card.SetFaceUp(true);
                }

                if(card != null)
                {
                    DisplayingCards.Remove(card);
                    receivingPlayer.ReceiveDisplayingCard(card);
                    cardAnimator.AddCardAnimation(card, receivingPlayer.NextCardPosition());
                    NumberOfDisplayingCards--;
                }
                else
                {
                    Debug.LogError("Unable to find displaying card.");
                }
                
            }
            RepositionDisplayingCards(cardAnimator);

        }

        public bool Equals(Player other)
        {
            if (PlayerId.Equals(other.PlayerId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void PutCards()
        {
            for (int i = 0; i < DisplayingCards.Count; i++)
            {
                if (DisplayingCards[i].Rank != Ranks.NoRanks)
                {
                    for (int j = i + 1; j < DisplayingCards.Count; j++)
                    {
                        if (DisplayingCards[j].transform.childCount < 1 && DisplayingCards[j].Rank != Ranks.NoRanks)
                        {
                            if (CheckForSimilarCards(DisplayingCards[i], DisplayingCards[j], out var card))
                            {
                                card.transform.position = DisplayingCards[i].transform.position + Vector3.up * Constants.PLAYER_CARD_UPPER_POSITION_OFFSET;
                                card.transform.SetParent(DisplayingCards[i].transform);
                                
                            }
                        }

                    }
                }
            }
        }

        private bool CheckForSimilarCards(Card firstCard, Card secondCard, out Card result)
        {
            if (firstCard.Rank == secondCard.Rank)
            {
                result = secondCard;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }
    }
}
