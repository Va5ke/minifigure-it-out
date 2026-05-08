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
    
    [Header("Audio")]
    [SerializeField] private AudioSource matchSound;

    [Header("Score")]
    [SerializeField] private TextMesh scoreLabel;
    private int _score = 0;
    [SerializeField] private TextMesh attemptsLabel;
    private int _attempts = 0;
    [SerializeField] private TextMesh comboLabel;
    private int _combo = 0;

    [SerializeField] private TextMesh timerText;

    private float _elapsedSeconds = 0f;
    private bool _paused = false;
    private bool _timerStarted = false;

    public bool IsPaused => _paused;

    public void Pause()
    {
        _paused = !_paused;
    }

    private void Update()
    {
        if (_paused || _score == 4 || !_timerStarted) return;
        _elapsedSeconds += Time.deltaTime;
        UpdateTimerLabel();
    }

    private void UpdateTimerLabel()
    {
        int totalSeconds = Mathf.FloorToInt(_elapsedSeconds);
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;
        int centiseconds = Mathf.FloorToInt((_elapsedSeconds - Mathf.Floor(_elapsedSeconds)) * 100);

        timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D2}", hours, minutes, seconds, centiseconds);
    }

    [Header("Loading UI")]
    [SerializeField] private GameObject loadingScreen;

    private const int TotalUniqueCards = 4;

    private List<MainCard> _allCards = new List<MainCard>();

    public bool CanReveal => _secondRevealedCard == null;
    private List<string> _imagePool;

    private void Start()
    {
        matchSound.volume = 0.3f;
    }

    public void StartGame()
    {
        _imagePool = BuildImagePool();
        originalCard.SetVisible(false);
        loadingScreen.SetActive(true);
        StartCoroutine(LoadImagesAndSetup());
    }

    private static List<string> BuildImagePool()
    {
        List<string> pool = new List<string>();

        // sh: 0001 to 1126 (4 digits)
        for (int i = 1; i <= 1126; i++)
            pool.Add($"sh{i:D4}");

        // dis: 001 to 192
        for (int i = 1; i <= 192; i++)
            pool.Add($"dis{i:D3}");

        // sim: 001 to 048
        for (int i = 1; i <= 48; i++)
            pool.Add($"sim{i:D3}");

        // hp: 134 to 616
        for (int i = 134; i <= 616; i++)
            pool.Add($"hp{i:D3}");

        // colhp: 01 to 39 (2 digits)
        for (int i = 1; i <= 39; i++)
            pool.Add($"colhp{i:D2}");

        // ftv: 001 to 007
        for (int i = 1; i <= 7; i++)
            pool.Add($"ftv{i:D3}");

        // iaj: 046 to 056
        for (int i = 46; i <= 56; i++)
            pool.Add($"iaj{i:D3}");

        // op: 001 to 025
        for (int i = 1; i <= 25; i++)
            pool.Add($"op{i:D3}");

        // ow: 001 to 017
        for (int i = 1; i <= 17; i++)
            pool.Add($"ow{i:D3}");

        // son: 001 to 033
        for (int i = 1; i <= 33; i++)
            pool.Add($"son{i:D3}");

        // bob: 001 to 038, with bob017 -> bob017b
        for (int i = 1; i <= 38; i++)
        {
            if (i == 17)
                pool.Add("bob017b");
            else
                pool.Add($"bob{i:D3}");
        }

        // tnt: 001 to 053
        for (int i = 1; i <= 53; i++)
            pool.Add($"tnt{i:D3}");

        // lor: 001 to 111
        for (int i = 1; i <= 111; i++)
            pool.Add($"lor{i:D3}");

        // scd: 001 to 011
        for (int i = 1; i <= 11; i++)
            pool.Add($"scd{i:D3}");

        // collt: 01 to 12 (2 digits)
        for (int i = 1; i <= 12; i++)
            pool.Add($"collt{i:D2}");

        // colsh: 01 to 16 (2 digits)
        for (int i = 1; i <= 16; i++)
            pool.Add($"colsh{i:D2}");

        // colmar: 01 to 24 (2 digits)
        for (int i = 1; i <= 24; i++)
            pool.Add($"colmar{i:D2}");

        // coltm: 01 to 12 (2 digits)
        for (int i = 1; i <= 12; i++)
            pool.Add($"coltm{i:D2}");

        // idea
        HashSet<int> ideaIds = new HashSet<int>();
        ideaIds.Add(49); ideaIds.Add(50);
        for (int i = 86; i <= 90; i++) ideaIds.Add(i);
        ideaIds.Add(192); ideaIds.Add(193);
        for (int i = 199; i <= 202; i++) ideaIds.Add(i);
        for (int i = 13; i <= 19; i++) ideaIds.Add(i);
        for (int i = 56; i <= 62; i++) ideaIds.Add(i);
        for (int i = 92; i <= 96; i++) ideaIds.Add(i);
        for (int i = 107; i <= 121; i++) ideaIds.Add(i);
        ideaIds.Add(44); /* skip 45 */ ideaIds.Add(46); ideaIds.Add(47); ideaIds.Add(48);
        for (int i = 73; i <= 78; i++) ideaIds.Add(i);

        foreach (int id in ideaIds)
            pool.Add($"idea{id:D3}");

        return pool;
    }

    private List<string> PickRandomImageKeys(int count)
    {
        HashSet<int> chosenIndices = new HashSet<int>();
        while (chosenIndices.Count < count)
            chosenIndices.Add(Random.Range(0, _imagePool.Count));

        List<string> result = new List<string>();
        foreach (int idx in chosenIndices)
            result.Add(_imagePool[idx]);
        return result;
    }

    private IEnumerator LoadImagesAndSetup()
    {
        List<string> chosenKeys = PickRandomImageKeys(TotalUniqueCards);
        List<CardData> deck = new List<CardData>();

        for (int i = 0; i < chosenKeys.Count; i++)
        {
            string url = $"https://img.bricklink.com/ItemImage/MN/0/{chosenKeys[i]}.png";
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

        loadingScreen.SetActive(false);
        foreach (MainCard card in _allCards)
            card.SetVisible(true);

        _timerStarted = true;
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
            _attempts++;
            attemptsLabel.text = "Attempts: " + _attempts;
            StartCoroutine(CheckCardMatchCoroutine());
        }
    }

    private IEnumerator CheckCardMatchCoroutine()
    {
        if (_firstRevealedCard.Id == _secondRevealedCard.Id)
        {
            _score++;
            scoreLabel.text = "Score: " + _score;
            _combo++;
            if (_combo > 1) comboLabel.text = "Combo! x" + _combo;
            matchSound.Play();
        }
        else
        {
            _combo = 0;
            comboLabel.text = "";
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

