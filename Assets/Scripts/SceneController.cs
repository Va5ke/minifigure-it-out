using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class SceneController : MonoBehaviour
{
    [Header("Cards Layout")]
    private const int gridRows = 2;
    private const int gridColumns = 4;
    private const float offsetX = 4f;
    private const float offsetY = 5f;

    [Header("Cards")]
    [SerializeField] private MainCard originalCard;
    private MainCard _firstRevealedCard;
    private MainCard _secondRevealedCard;

    [Header("Gameplay Settings")]
    [Range(0.5f, 5f)]
    [SerializeField] private float mismatchRevealDurationInSeconds = 1f;

    [Header("Score")]
    [SerializeField] private TextMesh scoreLabel;
    private int _score = 0;

    [Header("Loading UI")]
    [SerializeField] private GameObject loadingScreen;

    private const int TotalUniqueCards = 4;
    private const int MinImageId = 1;
    private const int MaxImageId = 1126;
    private const string ImageUrlTemplate = "https://img.bricklink.com/ItemImage/MN/0/sh{0:D4}.png";

    private List<MainCard> _allCards = new List<MainCard>();

    public bool CanReveal => _secondRevealedCard == null;

    private void Start()
    {
        // Hide the original card completely until the grid is ready
        originalCard.SetVisible(false);

        loadingScreen.SetActive(true);
        StartCoroutine(LoadImagesAndSetup());
    }

    private IEnumerator LoadImagesAndSetup()
    {
        List<int> chosenIds = PickRandomImageIds(TotalUniqueCards);
        List<CardData> deck = new List<CardData>();

        for (int i = 0; i < chosenIds.Count; i++)
        {
            string url = string.Format(ImageUrlTemplate, chosenIds[i]);
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                yield return request.SendWebRequest();

                Texture2D texture;
                if (request.result == UnityWebRequest.Result.Success)
                {
                    texture = DownloadHandlerTexture.GetContent(request);
                }
                else
                {
                    Debug.LogWarning($"Failed to load {url}: {request.error}. Using placeholder.");
                    texture = MakePlaceholderTexture(i);
                }

                CardData cardData = new CardData { id = i, face = texture };
                deck.Add(cardData);
                deck.Add(cardData);
            }
        }

        ShuffleDeck(deck);
        PlaceCards(deck);

        // All cards are placed — hide loading screen and reveal the grid
        loadingScreen.SetActive(false);
        foreach (MainCard card in _allCards)
        {
            card.SetVisible(true);
        }
    }

    private List<int> PickRandomImageIds(int count)
    {
        HashSet<int> chosen = new HashSet<int>();
        while (chosen.Count < count)
            chosen.Add(Random.Range(MinImageId, MaxImageId + 1));
        return new List<int>(chosen);
    }

    private Texture2D MakePlaceholderTexture(int colorIndex)
    {
        Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow };
        Texture2D tex = new Texture2D(2, 2);
        Color c = colors[colorIndex % colors.Length];
        tex.SetPixels(new Color[] { c, c, c, c });
        tex.Apply();
        return tex;
    }

    private static void ShuffleDeck(List<CardData> deck)
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (deck[i], deck[randomIndex]) = (deck[randomIndex], deck[i]);
        }
    }

    private void PlaceCards(List<CardData> deck)
    {
        Vector3 startPos = originalCard.transform.position;
        int cardIndex = 0;

        for (int col = 0; col < gridColumns; col++)
        {
            for (int row = 0; row < gridRows; row++)
            {
                MainCard card = (cardIndex == 0)
                    ? originalCard
                    : Instantiate(originalCard);

                card.SetUpCard(deck[cardIndex]);

                float x = startPos.x + col * offsetX;
                float y = startPos.y + row * offsetY;
                card.transform.position = new Vector3(x, y, startPos.z);

                _allCards.Add(card);
                cardIndex++;
            }
        }
    }

    public void RevealCard(MainCard card)
    {
        if (_firstRevealedCard == null)
        {
            _firstRevealedCard = card;
        }
        else
        {
            _secondRevealedCard = card;
            StartCoroutine(CheckCardMatchCoroutine());
        }
    }

    private IEnumerator CheckCardMatchCoroutine()
    {
        if (_firstRevealedCard.Id == _secondRevealedCard.Id)
        {
            _score++;
            scoreLabel.text = "Score: " + _score;
        }
        else
        {
            yield return new WaitForSeconds(mismatchRevealDurationInSeconds);
            _firstRevealedCard.Unreveal();
            _secondRevealedCard.Unreveal();
        }

        _firstRevealedCard = null;
        _secondRevealedCard = null;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}