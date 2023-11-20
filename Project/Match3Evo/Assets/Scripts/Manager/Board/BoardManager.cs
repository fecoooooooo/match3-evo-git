using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
using UnityEditor;
using System.Linq;
using UnityEngine.Events;

namespace Match3_Evo
{
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] List<int> columnTopRow = new List<int>();
        [SerializeField] BoardOverride boardOverride;

        [Required, GUIColor(0.6f, 1, 0.4f)] 
        public GameParameters gameParameters;
        public float fieldAnimationFPS = 20;
        [Range(0,.5f)]
        public float animationProbability = .15f;
        public float shiftTimeInSeconds = .5f;
        [Required, FoldoutGroup("Playfield Size"), GUIColor(0.6f, 1, 0.4f)] public int rows, columns;
        public float transitionSpeed, transitionMaxSpeed;
        public AnimationCurve fieldBounceCurve;

        public List<FieldDataEvo> FieldData;
        public Sprite dnsSprite;
        public int dnsCount;
        public Sprite treasureSprite;
        public int treasureCount;
        public int treasureScore;

        public bool InputEnabled { get; set; } = true;

        public FieldDataTier GetFieldDataForFieldType(int fieldVariant, int evoLvl)
        {
            return FieldData[fieldVariant].fieldDataTiers[evoLvl];
        }

        public MatchData[] MatchDatas;
        [Tooltip("Extra 5x5 Breakable combinations. Basic 3 matching elements in column and extra elements relative to the starting field from the 3 basic matching fields! SwapIndex is the center of the 5x5 break.")]
        public MatchData[] ExactMatchDatas;

        [Required, FoldoutGroup("Variables"), GUIColor(1, 0.8f, 0)] public Camera gameCamera;
        [Required, FoldoutGroup("Variables"), GUIColor(1, 0.8f, 0)] public Text infoUIPrefab;
        [Required, FoldoutGroup("Variables"), GUIColor(1, 0.8f, 0)] public ScoreFX scoreFXPrefab;
        [FoldoutGroup("Variables"), GUIColor(1, 0.8f, 0)] public CountDown countDown;
        [Required, FoldoutGroup("Variables"), GUIColor(1, 0.8f, 0)] public PauseMenu pauseMenu;
        [Required, FoldoutGroup("Variables"), GUIColor(1, 0.8f, 0)] public CanvasGroupFade boardCanvasGroupFade;
        [Required, FoldoutGroup("Variables"), GUIColor(1, 0.8f, 0)] public RectTransform fieldUIParent, scoreFXEndPosition, topFXParent;
        [Required, FoldoutGroup("Variables"), GUIColor(1, 0.8f, 0)] public GraphicRaycaster graphicRaycaster;
        [Required, FoldoutGroup("Variables"), SerializeField, GUIColor(1, 0.8f, 0)] Text debugStateText;
        [Required, FoldoutGroup("Variables"), SerializeField, GUIColor(1, 0.8f, 0)] FieldUI fieldUIPrefab;
        [Required, FoldoutGroup("Variables"), SerializeField, GUIColor(1, 0.8f, 0)] PoolObject breakBackgroundPrefab, lineLightning, boxLightning;
        [Required, FoldoutGroup("Variables"), SerializeField, GUIColor(1, 0.8f, 0)] RectTransform breakBackgroundParent, lightningParent;

        public LevelBg levelBg;

        [HideInInspector] public Transform collectedTileRoot;

        [HideInInspector] public int[] fieldStateCounter;
        [HideInInspector] public float breakDelayTime = 1.0f;
        [HideInInspector] public float breakDelayTimeFast = 0.5f;
        [HideInInspector] public float canvasGroupFadeSpeed = 4f;
        [HideInInspector] public float fieldStateGlobalChangeTime = 2f;
        [HideInInspector] public float fieldStateGlobalIdealTime = 2f;
        [HideInInspector] public float fieldSize;

        ColumnFeed[] columnFeeds;
        List<Field> fieldsList = new List<Field>();

        int minimumFieldCountForBreak = 3;

        bool gameRunning = false;
        bool gameStarted = false;
        bool endGame = false;
        int comboCount;
        // int lastComboBreakColumn = 0;

        public Field[,] Fields { get; private set; }

        public int Columns { get { return columns; } }

        public bool GameRunning { get { return gameRunning; } }

        public bool CanClickOnField { get; set; }
		public bool DebugEnabled { get; internal set; }
		public bool DnsMeterFilled { get; private set; }

		bool showHint;

        float currentWaitTillInputEnabled = 0.5f;
        readonly float WAIT_TILL_INPUT_ENABLED_TIME = 0.5f;
        bool isShifting = false;
            
        public const int DECIDE_EVOLUTION_LEVEL = 4;
        [HideInInspector]
        public int[] currentEvolutionLvlPerVariant;
        [HideInInspector]
        public int[] currentMergeCountToNextEvolvePerVariant;

        public MergeEvent mergeEvent = new MergeEvent();
        public EvolutionDecidedEvent evolutionDecidedEvent = new EvolutionDecidedEvent();

        MapPregenerator mapPregenerator;

        BottomFeedMap bottomFeedMap;

        void Awake()
        {
            CanClickOnField = true;
            GM.boardMng = this;
            fieldStateCounter = new int[Enum.GetNames(typeof(EnumFieldState)).Length];
            fieldStateCounter[(int)EnumFieldState.Useable] = rows * columns;
            fieldStateGlobalIdealTime = gameParameters.hintTime;

            for (int i = 0; i < columns; i++)
                columnTopRow.Add(0);

            TopRowInit();
        }

        void Evolve(int evolvingVariant)
		{
            if (currentEvolutionLvlPerVariant[evolvingVariant] + 1 < DECIDE_EVOLUTION_LEVEL)
			{
                currentEvolutionLvlPerVariant[evolvingVariant]++;
                int evoLvlForVariant = currentEvolutionLvlPerVariant[evolvingVariant];
                currentMergeCountToNextEvolvePerVariant[evolvingVariant] = gameParameters.mergesToNextEvolution[evoLvlForVariant];
            }
            else if (currentEvolutionLvlPerVariant[evolvingVariant] + 1 == DECIDE_EVOLUTION_LEVEL)
			{
                currentEvolutionLvlPerVariant[evolvingVariant]++;

				if (DnsMeterFilled)
                    currentEvolutionLvlPerVariant[evolvingVariant]++;

                evolutionDecidedEvent.Invoke(evolvingVariant);
			}

            int newEvoLvl = currentEvolutionLvlPerVariant[evolvingVariant];

            bottomFeedMap.Evolve(evolvingVariant, newEvoLvl);
            foreach (var cf in columnFeeds)
                cf.Evolve(evolvingVariant, newEvoLvl);

            foreach(var f in Fields)
			{
                if (f.EvoLvl < newEvoLvl && evolvingVariant == f.FieldVariant)
                    f.FieldType = Field.EvoLvlAndVariantToType(newEvoLvl, evolvingVariant);
            }
		}

        void Start()
        {
            mergeEvent.AddListener(OnMergeEvent);

            currentEvolutionLvlPerVariant = new int[gameParameters.tileVariantMax];
            currentMergeCountToNextEvolvePerVariant = new int[gameParameters.tileVariantMax];

            for (int i = 0; i < currentMergeCountToNextEvolvePerVariant.Length; ++i)
                currentMergeCountToNextEvolvePerVariant[i] = gameParameters.mergesToNextEvolution[0];

            Canvas.ForceUpdateCanvases();
            StartCoroutine(InitializeGame());
            GM.soundMng.StopAll();
            GM.soundMng.PlayDelayed(EnumSoundID.GameMusic, 3f);
        }

        IEnumerator InitializeGame()
        {
            yield return new WaitForEndOfFrame();

            //gameParameters.TestUpdateFromSkillz();    //Test do not forget to comment out!!!
            if (GM.IsSkillzMatchInProgress())
            {
                Debug.Log("InitializeGameUpdateFromSkillz");
                gameParameters.UpdateFromSkillz(GM.GetMatchRules());
            }

            GM.scoreMng.Initialize();

            GM.timeMng.InitGameTime();
            Fields = new Field[rows, columns];
            columnFeeds = new ColumnFeed[columns];

            fieldSize = fieldUIParent.rect.width / columns;

            levelBg.SetSize();

            Vector2 lvFieldPosition;
            for (int lvColumnIndex = 0; lvColumnIndex < columns; lvColumnIndex++)
            {
                for (int lvRowIndex = 0; lvRowIndex < rows; lvRowIndex++)
                {
                    FieldUI lvFieldUI = Instantiate(fieldUIPrefab, fieldUIParent);

                    lvFieldPosition = new Vector2(fieldSize * lvColumnIndex, -fieldSize * lvRowIndex);
                    Fields[lvRowIndex, lvColumnIndex] = new Field(lvRowIndex, lvColumnIndex, FieldType.V1_E0, 1, lvFieldPosition, lvFieldUI);
                    lvFieldUI.Initialize(Fields[lvRowIndex, lvColumnIndex]);
                    lvFieldUI.ResetPosition();
                    fieldsList.Add(Fields[lvRowIndex, lvColumnIndex]);
                }

                columnFeeds[lvColumnIndex] = new ColumnFeed(lvColumnIndex);
            }

            for (int lvColumnIndex = 0; lvColumnIndex < columns; lvColumnIndex++)
            {
                for (int lvRowIndex = rows - 1; lvRowIndex >= 0; lvRowIndex--)
                    Fields[lvRowIndex, lvColumnIndex].FindRelations();
            }

            if (boardOverride != null)
            {
                boardOverride.Override();
                boardOverride = null;
            }

            mapPregenerator = new MapPregenerator();
            mapPregenerator.PregenerateToColumns(columnFeeds);
            mapPregenerator.PregenerateBottomFeedMap(out bottomFeedMap);

            for (int lvColumnIndex = 0; lvColumnIndex < columns; lvColumnIndex++)
            {
                for (int lvRowIndex = rows - 1; lvRowIndex >= 0; lvRowIndex--)
                    Fields[lvRowIndex, lvColumnIndex].FieldState = EnumFieldState.Empty;

                FeedColumn(lvColumnIndex, true);
            }

            fieldStateCounter[(int)EnumFieldState.Empty] = 0;
            fieldStateCounter[(int)EnumFieldState.Useable] = 0;
            fieldStateCounter[(int)EnumFieldState.Hidden] = 0;
            SetRowLocked(rows - 2);
            SetRowUnBreakAble(rows - 1);

            PrepareGameStart();
        }



		private void Update()
        {
			//DebugState();
            if (gameStarted)
            {
                HandleInputEnabled();

                HandleFieldStateChange();

                BreakNewlyFallendFieldsIfPossible();
             
                CheckComboBreak();

                ShiftIfPossible();
            }

            HandleDebug();
        }

		private void HandleDebug()
		{
            if (Input.GetKeyDown(KeyCode.D))
                DebugEnabled = !DebugEnabled;
        }

    private void HandleInputEnabled()
		{
            int breakableFields = Fields?.Cast<Field>()?.ToArray()?.Count(x => x.FieldState == EnumFieldState.Break) ?? int.MaxValue;
            bool thereAreFieldsToBreak = breakableFields > 0;

            if (thereAreFieldsToBreak || isShifting)
                currentWaitTillInputEnabled = WAIT_TILL_INPUT_ENABLED_TIME;
            else
                currentWaitTillInputEnabled -= Time.deltaTime;

            InputEnabled = currentWaitTillInputEnabled <= 0;
        }

        private void ShiftIfPossible()
		{
            if (InputEnabled && IsRowUnlocked(rows - 2) && !isShifting || Input.GetKeyDown(KeyCode.A))
			{
                isShifting = true;

                Vector2 startPosition = Fields[rows - 1, 0].fieldPosition + new Vector2(0, -fieldSize);
                Field[] newFields = new Field[columns];

                int[] newFieldTypes = bottomFeedMap.PopRow();

                for(int i = 0; i < columns; ++i)
				{
                    FieldUI fieldUI = Instantiate(fieldUIPrefab, fieldUIParent);
                    Vector2 fieldPos = startPosition + new Vector2(fieldSize * i, 0);

                    int fieldVariant;
                    bool verticalMatch;
                    bool horizontalMatch;
                    do
                    {
                        fieldVariant = GM.GetRandom(0, gameParameters.TileVariantMax());
                        verticalMatch = Fields[rows - 1, i].FieldVariant == Fields[rows - 2, i].FieldVariant && Fields[rows - 1, i].FieldVariant == fieldVariant;
                        horizontalMatch = 1 < i && newFields[i - 1].FieldVariant == newFields[i - 2].FieldVariant && newFields[i - 1].FieldVariant == fieldVariant;
                    } while (verticalMatch || horizontalMatch);

                    Field field = new Field(-1, -1, (FieldType)newFieldTypes[i], 1, fieldPos, fieldUI);
                    fieldUI.transform.position = fieldPos;
                    fieldUI.Initialize(field);
                    fieldUI.ResetPosition();
                    fieldUI.SetLocked();

                    newFields[i] = field;
                }
                
                StartCoroutine(ShiftBoard(newFields));
            }
        }

        private IEnumerator ShiftBoard(Field[] newFields)
        {
            float t = 0;
            float levelBgStartingY = levelBg.rectTransform.anchoredPosition.y;
            float fieldStartingY = fieldUIParent.anchoredPosition.y;

            while (t < shiftTimeInSeconds)
            {
                float t01 = t / shiftTimeInSeconds;
                float deltaY = t01 * fieldSize;
                fieldUIParent.anchoredPosition = new Vector2(fieldUIParent.anchoredPosition.x, fieldStartingY + deltaY);
                levelBg.rectTransform.anchoredPosition = new Vector2(levelBg.rectTransform.anchoredPosition.x, levelBgStartingY + deltaY);

                t += Time.deltaTime;
                yield return null;
            }

            fieldUIParent.anchoredPosition = new Vector2(fieldUIParent.anchoredPosition.x, fieldStartingY);
            levelBg.rectTransform.anchoredPosition = new Vector2(levelBg.rectTransform.anchoredPosition.x, levelBgStartingY + fieldSize);

            for (int columnIndex = 0; columnIndex < columns; columnIndex++)
            {
                for (int rowIndex = 0; rowIndex < rows - 1; rowIndex++)
                {
                    Fields[rowIndex, columnIndex].FieldType = Fields[rowIndex + 1, columnIndex].FieldType;
                    Fields[rowIndex, columnIndex].fieldUI.Initialize(Fields[rowIndex, columnIndex]);
                }
            }

            for (int columnIndex = 0; columnIndex < columns; columnIndex++)
			{
                Fields[rows - 1, columnIndex].FieldType = newFields[columnIndex].FieldType;
                Fields[rows - 1, columnIndex].fieldUI.Initialize(Fields[rows - 1, columnIndex]);
                Destroy(newFields[columnIndex].fieldUI.gameObject);
			}

            SetRowLocked(rows - 2);

            isShifting = false;
        }

        void HandleFieldStateChange()
		{
            fieldStateGlobalChangeTime -= Time.deltaTime;

            if (fieldStateGlobalIdealTime > 0 && fieldStateGlobalIdealTime - Time.deltaTime <= 0f)
                fieldStateGlobalIdealTime = 0;
            else
                fieldStateGlobalIdealTime -= Time.deltaTime;

            if (fieldStateGlobalChangeTime < 0f && fieldStateCounter[(int)EnumFieldState.Useable] < columns * rows && gameRunning)
            {
                int lvEmpty = 0;
                for (int i = 0; i < fieldsList.Count; i++)
                {
                    if (fieldsList[i].FieldState == EnumFieldState.Empty)
                        lvEmpty++;
                    else
                    {
                        fieldsList[i].FieldState = EnumFieldState.Useable;
                        fieldsList[i].fieldUI.ResetPosition();
                    }
                }

                for (int i = 0; i < fieldStateCounter.Length; i++)
                    fieldStateCounter[i] = 0;

                fieldStateCounter[(int)EnumFieldState.Useable] = columns * rows - lvEmpty;
                fieldStateCounter[(int)EnumFieldState.Empty] = lvEmpty;
            }

            if (fieldStateCounter[(int)EnumFieldState.Empty] > 0)
            {
                for (int i = 0; i < columns; i++)
                    FeedColumn(i);
            }
        }

		private void BreakNewlyFallendFieldsIfPossible()
		{
            List<Mergeable> lvBreakable = FindBreakableFields();
            BreakMergeables(lvBreakable);
        }

		public void TopRowInit()
        {
            for (int i = 0; i < columnTopRow.Count; i++)
                columnTopRow[i] = 1;
        }

        void PrepareGameStart()
        {
#if UNITY_EDITOR
            if (GM.timeMng.disableCountDown && !GM.Instance.tutorialGame)
            {
                StartGame();
                return;
            }
#endif
            if (GM.Instance.tutorialGame || GM.Instance.firstLoadUp)
            {
                //StartGame();
                //countDown.OnCountDownEnded();
                //pauseMenu.ChangeUIAsTutorial();
                gameRunning = true;
                OnPauseGame();
                pauseMenu.OpenTutorial();
                //GM.scoreMng.ChangeUIAsTutorial();
            }
            else
                countDown.Init();
        }

        #region ExternalCalls

        public void StartGame()
        {
            gameRunning = true;
            gameStarted = true;

            if (startGameDelegate != null)
                startGameDelegate.Invoke();
        }

        public void OnPauseGame()
        {
            if (gameRunning)
            {
                gameRunning = false;
                pauseMenu.Show();
            }
        }

        public void OnResumeGame()
        {
            pauseMenu.Hide();
            gameRunning = true;
        }

        public void OnLeaveGame()
        {
            if (GM.IsSkillzMatchInProgress())
                OnSubmitScore();
            else
                SceneManager.LoadScene(GM.Instance.menuSceneName);
        }

        public void OnSubmitScore()
        {
            if (GM.IsSkillzMatchInProgress())
                GM.FinishTournament((float)GM.scoreMng.gameScore);
            else
                SceneManager.LoadScene(GM.Instance.menuSceneName);
        }

        public void OnSwapFields(Field _field, EnumSwapDirection _swapDirection, bool _hintSwap = false)
        {
            if (gameRunning && _field.FieldState == EnumFieldState.Useable && CanClickOnField)
            {
                Field lvSwapFieldTo = null;
                bool lvSwapTopLeft = false;
                bool lvIsRow = false;

                if (_swapDirection == EnumSwapDirection.Right)
                {
                    if (_field.columnIndex < columns - 1)
                    {
                        lvSwapFieldTo = Fields[_field.rowIndex, _field.columnIndex + 1];
                        lvIsRow = true;
                    }
                }
                else if (_swapDirection == EnumSwapDirection.Left)
                {
                    if (_field.columnIndex > 0)
                    {
                        lvSwapFieldTo = Fields[_field.rowIndex, _field.columnIndex - 1];
                        lvSwapTopLeft = true;
                        lvIsRow = true;
                    }
                }
                else if (_swapDirection == EnumSwapDirection.Up)
                {
                    if (_field.rowIndex > 0)
                    {
                        lvSwapFieldTo = Fields[_field.rowIndex - 1, _field.columnIndex];
                        lvSwapTopLeft = true;
                    }
                }
                else if (_swapDirection == EnumSwapDirection.Down)
                {
                    if (_field.rowIndex < rows - 1 && false == Fields[_field.rowIndex + 1, _field.columnIndex].fieldUI.Locked)
					{
                        lvSwapFieldTo = Fields[_field.rowIndex + 1, _field.columnIndex];
					}
                }

                if (lvSwapFieldTo != null && lvSwapFieldTo.FieldState == EnumFieldState.Useable)
                {
                    if (!_hintSwap)
                        _field.SwapWithField(lvSwapFieldTo);
                    else
                    {
                        Mergeable lvHintSwapMergeable = new Mergeable(2, lvIsRow, _field.FieldType);
                        if (lvIsRow)
                            lvHintSwapMergeable.breakUIWidth = new Vector2(2, 1);
                        else
                            lvHintSwapMergeable.breakUIWidth = new Vector2(1, 2);

                        if (!lvSwapTopLeft)
                        {
                            lvHintSwapMergeable.AddField(_field);
                            lvHintSwapMergeable.AddField(lvSwapFieldTo);
                            lvHintSwapMergeable.TopLeftField = _field;
                        }
                        else
                        {
                            lvHintSwapMergeable.AddField(lvSwapFieldTo);
                            lvHintSwapMergeable.AddField(_field);
                            lvHintSwapMergeable.TopLeftField = lvSwapFieldTo;
                        }
                    }
                }
            }
            else
                GM.soundMng.Play(EnumSoundID.SwapWrong);
        }

        public void DebugState()
        {
            StringBuilder lvBuilder = new StringBuilder();
            for (int i = 0; i < fieldStateCounter.Length; i++)
                lvBuilder.Append((EnumFieldState)i).Append("=").Append(fieldStateCounter[i]).Append("||");

            debugStateText.text = lvBuilder.ToString();
        }

        #endregion

        public List<Mergeable> FindBreakableFields(bool _ignoreFieldState = false)
        {
            List<Mergeable> lvResult = new List<Mergeable>();
            int lvStartIndex;
            int lvEndIndex;

            for (int lvColumnIndex = 0; lvColumnIndex < columns; lvColumnIndex++)
            {
                lvStartIndex = -1;
                lvEndIndex = -1;

                for (int lvRowInxed = 0; lvRowInxed < rows; lvRowInxed++)
                {
                    if (lvStartIndex == -1)
                    {
                        if (_ignoreFieldState || Fields[lvRowInxed, lvColumnIndex].FieldState == EnumFieldState.Useable && false == Fields[lvRowInxed, lvColumnIndex].fieldUI.Unbreakable)
                            lvStartIndex = lvRowInxed;
                    }
                    else
                    {
                        if (!_ignoreFieldState && 
                            ((Fields[lvRowInxed, lvColumnIndex].FieldState != EnumFieldState.Useable) || Fields[lvStartIndex, lvColumnIndex].FieldType != Fields[lvRowInxed, lvColumnIndex].FieldType || Fields[lvRowInxed, lvColumnIndex].fieldUI.Unbreakable))
                        {
                            lvEndIndex = lvRowInxed - 1;
                            lvRowInxed--;
                        }

                        if (lvEndIndex == -1 && lvRowInxed == rows - 1)
                        {
                            lvEndIndex = lvRowInxed;
                        }
                    }

                    if (lvEndIndex >= 0)
                    {
                        if (lvEndIndex - lvStartIndex + 1 >= minimumFieldCountForBreak)
                        {
                            Mergeable lvMergeable = new Mergeable(lvEndIndex - lvStartIndex + 1, false, Fields[lvStartIndex, lvColumnIndex].FieldType);
                            for (int i = lvStartIndex; i <= lvEndIndex; i++)
                                lvMergeable.AddField(Fields[i, lvColumnIndex]);

                            if (lvMergeable.mergeableType == EnumMergeableType.Box)
                                lvMergeable.UpdateBoxFieldTo();

                             lvResult.Add(lvMergeable);
                        }

                        lvStartIndex = lvEndIndex = -1;
                    }
                }
            }

            for (int lvRowInxed = 0; lvRowInxed < rows; lvRowInxed++)
            {
                lvStartIndex = -1;
                lvEndIndex = -1;

                for (int lvColumnIndex = 0; lvColumnIndex < columns; lvColumnIndex++)
                {
                    if (lvStartIndex == -1)
                    {
                        if (_ignoreFieldState || Fields[lvRowInxed, lvColumnIndex].FieldState == EnumFieldState.Useable && false == Fields[lvRowInxed, lvColumnIndex].fieldUI.Unbreakable)
                            lvStartIndex = lvColumnIndex;
                    }
                    else
                    {
                        if ((!_ignoreFieldState && 
                            (Fields[lvRowInxed, lvColumnIndex].FieldState != EnumFieldState.Useable) || Fields[lvRowInxed, lvStartIndex].FieldType != Fields[lvRowInxed, lvColumnIndex].FieldType || Fields[lvRowInxed, lvColumnIndex].fieldUI.Unbreakable))
                        {
                            lvEndIndex = lvColumnIndex - 1;
                            lvColumnIndex--;
                        }

                        if (lvEndIndex == -1 && lvColumnIndex == columns - 1)
                        {
                            lvEndIndex = lvColumnIndex;
                        }
                    }

                    if (lvEndIndex >= 0)
                    {
                        if (lvEndIndex - lvStartIndex + 1 >= minimumFieldCountForBreak)
                        {
                            Mergeable lvMergeable = new Mergeable(lvEndIndex - lvStartIndex + 1, true, Fields[lvRowInxed, lvStartIndex].FieldType);
                            for (int i = lvStartIndex; i <= lvEndIndex; i++)
                                lvMergeable.AddField(Fields[lvRowInxed, i]);

                            if (lvMergeable.mergeableType == EnumMergeableType.Box)
                                lvMergeable.UpdateBoxFieldTo();

                            lvResult.Add(lvMergeable);
                        }

                        lvStartIndex = lvEndIndex = -1;
                    }
                }
            }

            List<Field> lvExactMatchs = new List<Field>(5);
            List<Mergeable> lvNewExactMergeable = new List<Mergeable>();

            for (int i = 0; i < lvResult.Count; i++)
            {
                if (!lvResult[i].isRow && lvResult[i].Fields.Count == minimumFieldCountForBreak)
                {
                    Field lvField = lvResult[i].Fields[0];

                    for (int j = 0; j < ExactMatchDatas.Length; j++)
                    {
                        if (ExactMatchDatas[j].GetExactMatchingFields(lvField, lvExactMatchs))
                        {
                            Mergeable lvMergeable = new Mergeable(5, false, lvField.FieldType);
                            lvMergeable.AddFieldRange(lvExactMatchs);
                            lvMergeable.AddFieldRange(lvResult[i].Fields);
                            lvMergeable.UpdateBoxFieldTo(lvResult[i].Fields[ExactMatchDatas[j].swapIndex]);
                            lvNewExactMergeable.Add(lvMergeable);
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < lvResult.Count; i++)
            {
                if (lvResult[i].Fields.Count == minimumFieldCountForBreak)
                {
                    for (int j = 0; j < lvNewExactMergeable.Count; j++)
                    {
                        if (lvNewExactMergeable[j].Fields.Contains(lvResult[i].Fields[0]) &&
                            lvNewExactMergeable[j].Fields.Contains(lvResult[i].Fields[1]) &&
                            lvNewExactMergeable[j].Fields.Contains(lvResult[i].Fields[2]))
                        {
                            lvResult.RemoveAt(i--);
                            break;
                        }
                    }
                }
            }

            if (lvNewExactMergeable.Count > 0)
                lvResult.AddRange(lvNewExactMergeable);

            return lvResult;
        }

        #region FindPossibleBreaks

        Field startField = null;
        Field swapField = null;
        Field goodField = null;
        int goodFieldCount = 0;

        private bool PossibleBreaks()
        {
            for (int lvColumns = 0; lvColumns < columns; lvColumns++)
            {
                for (int lvRows = 0; lvRows < rows; lvRows++)
                {
                    int lvFieldType = Fields[lvRows, lvColumns].FieldVariant;

                    for (int i = 0; i < MatchDatas.Length; i++)
                    {
                        bool lvFound = false;

                        for (int j = 0; j < MatchDatas[i].displacements.Length; j++)
                        {
                            int lvXDis = lvColumns + MatchDatas[i].displacements[j].x;
                            int lvYDis = lvRows + MatchDatas[i].displacements[j].y;
                            if (lvXDis < 0 || lvXDis >= columns)
                            {
                                lvFound = false;
                                break;
                            }

                            if (lvYDis < 0 || lvYDis >= rows)
                            {
                                lvFound = false;
                                break;
                            }

                            if (Fields[lvYDis, lvXDis].FieldVariant != lvFieldType)
                            {
                                lvFound = false;
                                break;
                            }
                            else
                                lvFound = true;

                        }

                        if (lvFound)
                        {
                            //Vector2Int disp = MatchDatas[i].displacements[MatchDatas[i].swapIndex];
                            //OnSwapFields(fields[y + disp.y, x + disp.x], MatchDatas[i].SwapDirection);
                            if (showHint)
                            {
                                showHint = false;
                                Vector2Int lvDisp = MatchDatas[i].displacements[MatchDatas[i].swapIndex];
                                OnSwapFields(Fields[lvRows + lvDisp.y, lvColumns + lvDisp.x], MatchDatas[i].SwapDirection, true);
                            }

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        Mergeable CheckPossibleBreak(bool _isRow)
        {
            Mergeable lvMergeable = new Mergeable(goodFieldCount + 1, false);

            if (_isRow)
            {
                if (swapField.Top != null && swapField.Top.FieldVariant == goodField.FieldVariant)
                    lvMergeable.possibleSwapData = new Mergeable.PossibleSwap(swapField, swapField.Top);
                else if (swapField.Bottom != null && swapField.Bottom.FieldVariant == goodField.FieldVariant)
                    lvMergeable.possibleSwapData = new Mergeable.PossibleSwap(swapField, swapField.Bottom);
                else if (goodFieldCount >= minimumFieldCountForBreak)
                    lvMergeable.possibleSwapData = new Mergeable.PossibleSwap(swapField, swapField.Left == startField ? startField : swapField.Right);
                else
                    lvMergeable = null;
            }
            else
            {
                if (swapField.Left != null && swapField.Left.FieldVariant == goodField.FieldVariant)
                    lvMergeable.possibleSwapData = new Mergeable.PossibleSwap(swapField, swapField.Left);
                else if (swapField.Right != null && swapField.Right.FieldVariant == goodField.FieldVariant)
                    lvMergeable.possibleSwapData = new Mergeable.PossibleSwap(swapField, swapField.Right);
                else if (goodFieldCount >= minimumFieldCountForBreak)
                    lvMergeable.possibleSwapData = new Mergeable.PossibleSwap(swapField, swapField.Top == startField ? startField : swapField.Bottom);
                else
                    lvMergeable = null;
            }

            if (lvMergeable != null)
            {
                for (int i = 0; i <= goodFieldCount; i++)
                {
                    if (_isRow)
                        lvMergeable.AddField(Fields[startField.rowIndex, startField.columnIndex + i]);
                    else
                        lvMergeable.AddField(Fields[startField.rowIndex + i, startField.columnIndex]);
                }

                return lvMergeable;
            }

            return null;
        }

        #endregion

        bool randomSwapping = false;
        IEnumerator RandomSwap()
        {
            randomSwapping = true;
            Instantiate(infoUIPrefab.gameObject, topFXParent).GetComponent<Text>().text = "No Moves\nRemaining";
            yield return new WaitForSeconds(1.5f);

            for (int lvRowIndex = 0; lvRowIndex < rows; lvRowIndex++)
            {
                for (int lvColumnIndex = 0; lvColumnIndex < columns; lvColumnIndex++)
                {
                    swapField = Fields[GM.GetRandom(0, rows), GM.GetRandom(0, columns)];

                    if (swapField != Fields[lvRowIndex, lvColumnIndex] && swapField.FieldVariant != Fields[lvRowIndex, lvColumnIndex].FieldVariant)
                        Fields[lvRowIndex, lvColumnIndex].RandomSwap(swapField);
                }
            }

            List<Mergeable> lvMergeables = FindBreakableFields(true);

            if (lvMergeables.Count > 0)
            {
                for (int index = 0; index < lvMergeables.Count; index++)
                {
                    Field lvReplaceField = lvMergeables[index].GetReplaceableField();

                    while (lvReplaceField != null)
                    {
                        if (lvMergeables[index].isRow)
                        {
                            if (lvReplaceField.Top != null && lvReplaceField.Top.NeighborsWithFieldType(lvReplaceField.FieldVariant) == 1 && lvReplaceField.NeighborsWithFieldType(lvReplaceField.Top.FieldVariant) == 0)
                                lvReplaceField.RandomSwap(lvReplaceField.Top);
                            else if (lvReplaceField.Bottom != null && lvReplaceField.Bottom.NeighborsWithFieldType(lvReplaceField.FieldVariant) == 1 && lvReplaceField.NeighborsWithFieldType(lvReplaceField.Bottom.FieldVariant) == 0)
                                lvReplaceField.RandomSwap(lvReplaceField.Bottom);
                            else
                            {
                                Field lvReplaceTo = FindRandomBreakNeutralField(lvMergeables, lvReplaceField);

                                if (lvReplaceTo != null)
                                    lvReplaceField.RandomSwap(lvReplaceTo);
                            }
                        }
                        else
                        {
                            if (lvReplaceField.Left != null && lvReplaceField.Left.NeighborsWithFieldType(lvReplaceField.FieldVariant) == 1 && lvReplaceField.NeighborsWithFieldType(lvReplaceField.Left.FieldVariant) == 0)
                                lvReplaceField.RandomSwap(lvReplaceField.Left);
                            else if (lvReplaceField.Right != null && lvReplaceField.Right.NeighborsWithFieldType(lvReplaceField.FieldVariant) == 1 && lvReplaceField.NeighborsWithFieldType(lvReplaceField.Right.FieldVariant) == 0)
                                lvReplaceField.RandomSwap(lvReplaceField.Right);
                            else
                            {
                                Field lvReplaceTo = FindRandomBreakNeutralField(lvMergeables, lvReplaceField);

                                if (lvReplaceTo != null)
                                    lvReplaceField.RandomSwap(lvReplaceTo);
                            }
                        }

                        lvReplaceField = lvMergeables[index].GetReplaceableField();
                    }
                }
            }

            randomSwapping = false;
        }

        Field FindRandomBreakNeutralField(List<Mergeable> _mergeables, Field _replaceField)
        {
            Field lvResultField = null;

            while (lvResultField == null)
            {
                lvResultField = Fields[GM.GetRandom(0, rows), GM.GetRandom(0, columns)];
                if (lvResultField.NeighborsWithFieldType(_replaceField.FieldVariant) == 0 && _replaceField.NeighborsWithFieldType(lvResultField.FieldVariant) == 0)
                {
                    for (int i = 0; i < _mergeables.Count; i++)
                    {
                        if (_mergeables[i].Fields.Contains(lvResultField))
                        {
                            lvResultField = null;
                            break;
                        }
                    }

                    if (lvResultField != null)
                        return lvResultField;
                }
                else
                    lvResultField = null;
            }

            return lvResultField;
        }

        public void CheckComboBreak()
        {
            if (!randomSwapping && fieldStateCounter[(int)EnumFieldState.Break] == 0
                && fieldStateCounter[(int)EnumFieldState.Empty] == 0
                && fieldStateCounter[(int)EnumFieldState.Move] == 0
                && fieldStateCounter[(int)EnumFieldState.Swap] == 0
                && fieldStateCounter[(int)EnumFieldState.SwapBack] == 0
                && fieldStateCounter[(int)EnumFieldState.Hidden] == 0)
            {
                if (boardOverride != null)
                {
                    boardOverride.Override();
                    boardOverride = null;
                }

                for (int rowIndex = 0; rowIndex < rows; rowIndex++)
                {
                    for (int columnIndex = 0; columnIndex < columns; columnIndex++)
                    {
                        if (Fields[rowIndex, columnIndex].FieldState != EnumFieldState.Useable)
                            Fields[rowIndex, columnIndex].ChangeFieldState(EnumFieldState.Useable);
                    }
                }

                List<Mergeable> lvMergables = FindBreakableFields();

                if (lvMergables.Count > 0)
                {
                    comboCount++;
                    //BreakMergeables(lvMergables);
                    GM.scoreMng.AddComboBonus(null, comboCount);
                }
                else
                {
                    comboCount = 0;
                    if (gameRunning && !PossibleBreaks())
                        StartCoroutine(RandomSwap());
                }
            }
        }

        public void BreakMergeables(List<Mergeable> _mergeables)
        {
            for (int i = 0; i < _mergeables.Count; i++)
            {
                if (_mergeables[i].mergeableType == EnumMergeableType.Line)
                {
                    if (_mergeables[i].isRow)
                    {
                        int lvRowIndex = _mergeables[i].Fields[0].rowIndex;

                        _mergeables[i].ClearFields();

                        for (int j = 0; j < columns; j++)
                            _mergeables[i].AddField(Fields[lvRowIndex, j]);

                        _mergeables[i].breakUIWidth = new Vector2(columns, 1);
                        _mergeables[i].TopLeftField = Fields[lvRowIndex, 0];
                    }
                    else
                    {
                        int lvColumnIndex = _mergeables[i].Fields[0].columnIndex;

                        _mergeables[i].ClearFields();

                        for (int j = 0; j < rows; j++)
                            _mergeables[i].AddField(Fields[j, lvColumnIndex]);

                        _mergeables[i].breakUIWidth = new Vector2(1, rows);
                        _mergeables[i].TopLeftField = Fields[0, lvColumnIndex];
                    }
                }

                if (_mergeables[i].mergeableType == EnumMergeableType.Box)
                {
                    _mergeables[i].ClearFields();

                    Vector2Int lvTopLeft = new Vector2Int(Mathf.Clamp(_mergeables[i].BoxField.columnIndex - 2, 0, columns - 1), Mathf.Clamp(_mergeables[i].BoxField.rowIndex - 2, 0, rows - 1));
                    Vector2Int lvBottomRight = new Vector2Int(Mathf.Clamp(_mergeables[i].BoxField.columnIndex + 2, 0, columns - 1), Mathf.Clamp(_mergeables[i].BoxField.rowIndex + 2, 0, rows - 1));

                    for (int row = lvTopLeft.y; row <= lvBottomRight.y; row++)
                    {
                        for (int column = lvTopLeft.x; column <= lvBottomRight.x; column++)
                            _mergeables[i].AddField(Fields[row, column]);
                    }

                    _mergeables[i].breakUIWidth = lvBottomRight - lvTopLeft + Vector2.one;
                    _mergeables[i].TopLeftField = Fields[lvTopLeft.y, lvTopLeft.x];
                }

                //Go backward to break the lowest tile first
                for (int j = _mergeables[i].Fields.Count - 1; j >= 0; j--)
                {
                    if (_mergeables[i].TopLeftField == null)
                    {
                        if (_mergeables[i].isRow)
                            _mergeables[i].breakUIWidth = new Vector2(_mergeables[i].Fields.Count, 1);
                        else
                            _mergeables[i].breakUIWidth = new Vector2(1, _mergeables[i].Fields.Count);

                        _mergeables[i].TopLeftField = _mergeables[i].Fields[0];
                    }

                    if (_mergeables[i].Fields.Count == minimumFieldCountForBreak)
                        _mergeables[i].Fields[j].Break(breakDelayTimeFast);
                    else
                        _mergeables[i].Fields[j].Break(breakDelayTime);

                    _mergeables[i].Fields[j].fieldUI.SetUnlockedIfNotUnbreakable();
                }

                
                mergeEvent.Invoke(_mergeables[i].Fields[0].FieldVariant);
                _mergeables[i].PlayBreakSound();
            }
        }

		#region boost
		public void UseBoost(BoostType boostType, FieldUI targetField)
        {
            switch (boostType)
            {
                case BoostType.Hint:
                    ShowHint();
                    break;
                case BoostType.ColorFrenzy:
                    ColorFrenzyBreak(targetField);
                    break;
                case BoostType.Hammer:
                    HammerBreak(targetField);
                    break;
                case BoostType.Shovel:
                    ShovelBreak(targetField);
                    break;
                case BoostType.Fire:
                    StartCoroutine(FireBreakRoutine(targetField));
                    break;
                case BoostType.Spiral:
                    SpiralBreak(targetField);
                    break;
                default:
                    throw new Exception("No such enum type");
            }
        }

        void ShowHint()
        {
            showHint = true;
        }

        private void ColorFrenzyBreak(FieldUI targetField)
        {
            List<Field> fieldsToBreak = new List<Field>();

            foreach (var f in Fields)
            {
                if (f.FieldVariant == targetField.Field.FieldVariant && false == f.fieldUI.Locked)
                    fieldsToBreak.Add(f);
            }

            BreakMultipleDelayed(fieldsToBreak);
        }

        public void HammerBreak(FieldUI targetField)
        {
            if(false == targetField.Locked)
                targetField.Field.Break(.1f);
        }

        private void ShovelBreak(FieldUI targetField)
        {
            List<Field> fieldsToBreak = new List<Field>();

            foreach (var f in Fields)
            {
                if (f.rowIndex == targetField.Field.rowIndex && false == f.fieldUI.Locked)
                    fieldsToBreak.Add(f);
            }

            BreakMultipleDelayed(fieldsToBreak);
        }

        IEnumerator FireBreakRoutine(FieldUI targetField)
        {
            HashSet<Field> fieldsToBreak = new HashSet<Field>();

            foreach (var f in Fields)
            {
                if (f.FieldVariant == targetField.Field.FieldVariant && false == f.fieldUI.Locked)
                {
                    fieldsToBreak.Add(f);
                    f.fieldUI.SetOnFire();
                }
            }

            bool anyFieldOnFire;

            do
            {
                yield return new WaitForSeconds(gameParameters.fireSpreadTime);

                HashSet<Field> newFieldsOnFire = new HashSet<Field>();

                foreach (var f in fieldsToBreak)
                {
                    bool fieldOnHorizontalOppositeSideOnFire = f.rowIndex + 2 < rows && f.fieldUI.OnFire && Fields[f.rowIndex + 2, f.columnIndex].fieldUI.OnFire;
                    bool fieldOnVerticalOppositeSideOnFire = f.columnIndex + 2 < columns && f.fieldUI.OnFire && Fields[f.rowIndex, f.columnIndex + 2].fieldUI.OnFire;
                    bool fieldOnDiagonalOppositeSideOnFire = f.rowIndex + 2 < rows && f.columnIndex + 2 < columns && f.fieldUI.OnFire && Fields[f.rowIndex + 2, f.columnIndex + 2].fieldUI.OnFire;

                    if (fieldOnHorizontalOppositeSideOnFire && false == Fields[f.rowIndex + 1, f.columnIndex].fieldUI.Locked)
                        newFieldsOnFire.Add(Fields[f.rowIndex + 1, f.columnIndex]);
                    if (fieldOnVerticalOppositeSideOnFire && false == Fields[f.rowIndex, f.columnIndex + 1].fieldUI.Locked)
                        newFieldsOnFire.Add(Fields[f.rowIndex, f.columnIndex + 1]);
                    if (fieldOnDiagonalOppositeSideOnFire && false == Fields[f.rowIndex + 1, f.columnIndex + 1].fieldUI.Locked)
                        newFieldsOnFire.Add(Fields[f.rowIndex + 1, f.columnIndex + 1]);
                }

                foreach (var nf in newFieldsOnFire)
                    nf.fieldUI.SetOnFire();

                fieldsToBreak.UnionWith(newFieldsOnFire);

                anyFieldOnFire = fieldsToBreak.Where(x => x.fieldUI.OnFire).Any();
            } while (anyFieldOnFire);

            BreakMultipleDelayed(fieldsToBreak.ToList());
        }

        //Vector2Int[] spiralBreakSteps = new Vector2Int[] { 
        //    new Vector2Int(0, -1),    --> egyet fel
        //    new Vector2Int(1, 0),     --> egyet jobbra
        //    new Vector2Int(0, 1), new Vector2Int(0, 1),   --> kettot le
        //    new Vector2Int(-1, 0), new Vector2Int(-1, 0), --> kettot balra
        //    new Vector2Int(0, -1), new Vector2Int(0, -1), new Vector2Int(0, -1),  --> hátmat fel
        //    ...
        //};

        private void SpiralBreak(FieldUI targetField)
        {
            List<Field> fieldsToBreak = new List<Field>();
            fieldsToBreak.Add(targetField.Field);

            Vector2Int currentFieldPos = new Vector2Int(targetField.Field.rowIndex, targetField.Field.columnIndex);

            int spiralSteps = 0;
            int spiralStepsInCurrentDirection = 1;
            Vector2Int spiralStepDirection = new Vector2Int(-1, 0);
            bool spiralRunning = true;

            while (spiralRunning)
            {
                for (int i = 0; i < spiralStepsInCurrentDirection; ++i)
                {
                    currentFieldPos += spiralStepDirection;
                    if (currentFieldPos.x < 0 || currentFieldPos.y < 0 || rows <= currentFieldPos.x || columns <= currentFieldPos.y)
                    {
                        spiralRunning = false;
                        break;
                    }

                    if(false == Fields[currentFieldPos.x, currentFieldPos.y].fieldUI.Locked)
                        fieldsToBreak.Add(Fields[currentFieldPos.x, currentFieldPos.y]);
                }

                ++spiralSteps;

                if (spiralSteps % 2 == 1)
                    spiralStepDirection *= -1;
                if (spiralSteps % 2 == 0)
                    ++spiralStepsInCurrentDirection;

                spiralStepDirection = new Vector2Int(spiralStepDirection.y, spiralStepDirection.x);
            }

            BreakMultipleDelayed(fieldsToBreak);
        }


        public void BreakMultipleDelayed(List<Field> fieldsToBreak)
        {
            int i = 0;
            foreach (var f in fieldsToBreak)
            {
                ++i;
                f.Break(gameParameters.timeTillFirstBreak + i * gameParameters.timeBetweenBreaks);
                //f.fieldUI.fieldImage.sprite = (Sprite)AssetDatabase.LoadAssetAtPath("Assets/Imports/Sprites/ui/CloseButton.png", typeof(Sprite));
            }
        }

        public void FeedColumn(int _columnIndex, bool _initCall = false)
        {
            int lvFirstRefillField = 0;

            for (int lvRowIndex = rows - 1; lvRowIndex >= 0; lvRowIndex--)
            {
                if (lvRowIndex == rows - 1 && Fields[lvRowIndex, _columnIndex].FieldState == EnumFieldState.Empty && !_initCall) {
                    columnTopRow[_columnIndex] = 0;
                    //Debug.Log("index: " + _columnIndex + ", value: " + columnTopRow[_columnIndex]);
                }

                Field lvFieldAbove = null;

                //feed column
                if (Fields[lvRowIndex, _columnIndex].FieldState == EnumFieldState.Empty)
                {
                    //if (lvRowIndex > 0 && _columnIndex > 0)
                    //{
                    //    fields[lvRowIndex, _columnIndex].fieldUI.debugText.text = "";
                    //    if (fields[lvRowIndex - 1, _columnIndex].FieldState != EnumFieldState.Empty)
                    //    {
                    //        int variant = fields[lvRowIndex - 1, _columnIndex].fieldVariant;
                    //        int match = 0;
                    //        if (fields[lvRowIndex - 1, _columnIndex - 1].FieldState != EnumFieldState.Empty && fields[lvRowIndex - 1, _columnIndex - 1].fieldVariant == variant) match++;
                    //        if (match > 0)
                    //        {
                    //            fields[lvRowIndex - 1, _columnIndex].fieldUI.debugText.text = match.ToString();
                    //            fields[lvRowIndex - 1, _columnIndex - 1].fieldUI.debugText.text = match.ToString();
                    //            fields[lvRowIndex - 1, _columnIndex].fieldUI.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
                    //            //fields[lvRowIndex - 1, _columnIndex].fieldUI.transform.lo = new Vector3(2.0f, 2.0f, 2.0f);
                    //            continue;
                    //        }
                    //        else
                    //        {
                    //            fields[lvRowIndex, _columnIndex].fieldUI.debugText.text = "";
                    //            fields[lvRowIndex - 1, _columnIndex].fieldUI.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    fields[lvRowIndex, _columnIndex].fieldUI.debugText.text = "";
                    //    fields[lvRowIndex, _columnIndex].fieldUI.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    //}

                    for (int lvRowAbove = lvRowIndex - 1; lvRowAbove >= 0 && lvFieldAbove == null; lvRowAbove--)
                    {
                        if (Fields[lvRowAbove, _columnIndex].FieldState == EnumFieldState.Useable || Fields[lvRowAbove, _columnIndex].FieldState == EnumFieldState.Move || Fields[lvRowAbove, _columnIndex].FieldState == EnumFieldState.ComboReady)
                        {
                            lvFieldAbove = Fields[lvRowAbove, _columnIndex];
                            Fields[lvRowIndex, _columnIndex].MoveFieldHere(lvFieldAbove);
                        }
                        else if (Fields[lvRowAbove, _columnIndex].FieldState != EnumFieldState.Empty && Fields[lvRowAbove, _columnIndex].FieldState != EnumFieldState.Hidden)
                            lvFieldAbove = Fields[lvRowAbove, _columnIndex];
                    }

                    //Add new tile to top
                    if (lvFieldAbove == null)
                    {
                        if (endGame)
                            continue;

                        lvFirstRefillField++;
                        Fields[lvRowIndex, _columnIndex].ChangeFieldState(EnumFieldState.Move);
                        Fields[lvRowIndex, _columnIndex].fieldUI.ResetPositionToRefil(lvFirstRefillField);
                        Fields[lvRowIndex, _columnIndex].FieldType = (FieldType)columnFeeds[_columnIndex].GetFieldType(Fields[lvRowIndex, _columnIndex].fieldUI);
                        Fields[lvRowIndex, _columnIndex].fieldUI.Initialize(Fields[lvRowIndex, _columnIndex]);
                        Fields[lvRowIndex, _columnIndex].fieldUI.gameObject.SetActive(true);
                    }
                }
            }
        }

        #endregion

		public void TreasureBreak(FieldUI targetField)
        {
            ScoreFX.CreateForTreasure(targetField.Field);
        }


        bool IsRowUnlocked(int row)
		{
            for (int col = 0; col < columns; ++col)
			{
                if (Fields[row, col].fieldUI.Locked && !Fields[row, col].SpecialType)
                    return false;
			}

            return true;
        }

        void SetRowLocked(int row)
		{
            for (int i = 0; i < columns; ++i)
                Fields[row, i].fieldUI.SetLocked();
		}

        private void SetRowUnBreakAble(int row)
        {
            for (int i = 0; i < columns; ++i)
                Fields[row, i].fieldUI.SetUnbreakableAndLocked();
        }

        void OnMergeEvent(int variant)
        {
            currentMergeCountToNextEvolvePerVariant[variant]--;

            if (currentMergeCountToNextEvolvePerVariant[variant] <= 0)
                Evolve(variant);
        }

        public IEnumerator ShowBreakBackground(Mergeable _mergeable)
        {
            BreakBackground lvBg = GM.Pool.GetObject<BreakBackground>(breakBackgroundPrefab);
            lvBg.Initialize(breakBackgroundParent, _mergeable);
            if (_mergeable.mergeableType == EnumMergeableType.Hint)
                yield break;

            yield return new WaitForSeconds(breakDelayTime * 0.25f);

            ScoreFX.Create(_mergeable);

            if (_mergeable.mergeableType == EnumMergeableType.Line)
            {
                Lightning lvLightning = GM.Pool.GetObject<Lightning>(lineLightning);
                lvLightning.Initialize(lightningParent, _mergeable);
            }
            else if (_mergeable.mergeableType == EnumMergeableType.Box)
            {
                RectTransform lvBoxLightningRect = GM.Pool.GetObject<RectTransform>(boxLightning);
                lvBoxLightningRect.SetParent(lightningParent, false);
                lvBoxLightningRect.anchoredPosition = _mergeable.BoxField.fieldPosition + new Vector2(0.5f * GM.boardMng.fieldSize, -0.5f * GM.boardMng.fieldSize);
                Lightning[] lvLightnings = lvBoxLightningRect.GetComponentsInChildren<Lightning>();

                for (int i = 0; i < lvLightnings.Length; i++)
                    lvLightnings[i].Initialize(null, _mergeable);

                lvBoxLightningRect.gameObject.SetActive(true);
            }
        }

        #region EndGame

        public void EndGame()
        {
            gameRunning = false;
            StartCoroutine(ScoreSummary());
        }

        IEnumerator ScoreSummary()
        {
            PauseButtonHandler lvPauseButton = FindObjectOfType<PauseButtonHandler>();
            if (lvPauseButton != null)
                lvPauseButton.gameObject.SetActive(false);

            Instantiate(infoUIPrefab.gameObject, topFXParent);
            yield return new WaitForSeconds(1.5f);

            while (fieldStateCounter[(int)EnumFieldState.Useable] != rows * columns)
                yield return new WaitForSeconds(0.5f);

            endGame = true;

            if (gameParameters.finaleClear)
            {
                List<Mergeable> lvMergeables = new List<Mergeable>();
                for (int lvColumnIndex = 0; lvColumnIndex < columns; lvColumnIndex++)
                {
                    lvMergeables.Add(new Mergeable(rows, false));

                    for (int lvRowIndex = 0; lvRowIndex < rows; lvRowIndex++)
                        lvMergeables[0].AddField(Fields[lvRowIndex, lvColumnIndex]);

                    BreakMergeables(lvMergeables);
                    FeedColumn(lvColumnIndex);
                    yield return new WaitForSeconds(0.25f);

                    lvMergeables.Clear();
                }
            }

            while (fieldStateCounter[(int)EnumFieldState.Empty] != rows * columns)
                yield return new WaitForSeconds(0.5f);

            yield return new WaitForSeconds(0.5f);

            GM.soundMng.Play(EnumSoundID.GameEnd);
            GM.scoreMng.ScoreSummary();
        }

		#endregion

		#region eventDelegates

		public delegate void StartGameDelegate();

        public StartGameDelegate startGameDelegate;

        public delegate void ClickOnFieldDelegate();

        public ClickOnFieldDelegate clickOnFieldDelegate;

        public delegate void MagnetSelectedDelegate(bool isActive);

        public MagnetSelectedDelegate magnetSelectedDelegate;

        public delegate void ColorTransitionEndedDelegate();

        public ColorTransitionEndedDelegate colorTransitionEndedDelegate;

        public delegate void ThereIsNoPossibleMergeDelegate();

        public ThereIsNoPossibleMergeDelegate thereIsNoPossibleMergeDelegate;

		#endregion
	}



    [Serializable]
    public class FieldDataEvo
	{
        public FieldDataTier[] fieldDataTiers;
	}

    [Serializable]
    public class FieldDataTier
    {
        public Sprite basic;
        public Sprite shadow;
        public List<Sprite> bubbleAnimation;
    }

    [Serializable]
    public class MatchData
    {
        public Vector2Int[] displacements;
        public int swapIndex = 0;
        public EnumSwapDirection SwapDirection = EnumSwapDirection.Down;

        public bool GetExactMatchingFields(Field _matchTo, List<Field> _matchingFields)
        {
            _matchingFields.Clear();

            bool lvFound = false;

            for (int i = 0; i < displacements.Length; i++)
            {
                int lvXDis = _matchTo.columnIndex + displacements[i].x;
                int lvYDis = _matchTo.rowIndex + displacements[i].y;

                if (lvXDis < 0 || lvXDis >= GM.boardMng.columns)
                {
                    lvFound = false;
                    break;
                }

                if (lvYDis < 0 || lvYDis >= GM.boardMng.rows)
                {
                    lvFound = false;
                    break;
                }

                if (GM.boardMng.Fields[lvYDis, lvXDis].FieldType != _matchTo.FieldType)
                {
                    lvFound = false;
                    break;
                }
                else
                {
                    _matchingFields.Add(GM.boardMng.Fields[lvYDis, lvXDis]);
                    lvFound = true;
                }
            }

            return lvFound;
        }
    }

    public class MergeEvent : UnityEvent<int>{}
    public class EvolutionDecidedEvent : UnityEvent<int>{}
}