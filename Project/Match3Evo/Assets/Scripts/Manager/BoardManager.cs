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

        public List<FieldData> FieldDatas;
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

        Field[,] fields;

        public Field[,] Fields { get { return fields; } }

        public int Columns { get { return columns; } }

        public bool GameRunning { get { return gameRunning; } }

        public bool CanClickOnField { get; set; }

        bool showHint;

        float currentWaitTillShift = 0.5f;
        readonly float WAIT_TILL_SHIFT_TIME = 0.5f;
        bool isShifting = false;

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

        void Start()
        {
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
            fields = new Field[rows, columns];
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
                    fields[lvRowIndex, lvColumnIndex] = new Field(lvRowIndex, lvColumnIndex, GM.GetRandom(0, gameParameters.TileVariantMax()), 1, lvFieldPosition, lvFieldUI);
                    lvFieldUI.Initialize(fields[lvRowIndex, lvColumnIndex]);
                    lvFieldUI.ResetPosition();
                    fieldsList.Add(fields[lvRowIndex, lvColumnIndex]);
                }

                columnFeeds[lvColumnIndex] = new ColumnFeed(lvColumnIndex);
            }

            for (int lvColumnIndex = 0; lvColumnIndex < columns; lvColumnIndex++)
            {
                for (int lvRowIndex = rows - 1; lvRowIndex >= 0; lvRowIndex--)
                    fields[lvRowIndex, lvColumnIndex].FindRelations();
            }

            StartupReplaceBreakable();
            StartupReplaceBreakable();

            if (boardOverride != null)
            {
                boardOverride.Override();
                boardOverride = null;
            }

            for (int lvColumnIndex = 0; lvColumnIndex < columns; lvColumnIndex++)
            {
                for (int lvRowIndex = rows - 1; lvRowIndex >= 0; lvRowIndex--)
                {
                    columnFeeds[lvColumnIndex].AddField(fields[lvRowIndex, lvColumnIndex].fieldVariant);
                    fields[lvRowIndex, lvColumnIndex].FieldState = EnumFieldState.Empty;
                }

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
                HandleFieldStateChange();

                BreakNewlyFallendFieldsIfPossible();
             
                CheckComboBreak();

                ShiftIfPossible();
            }
        }

		private void ShiftIfPossible()
		{
            int breakableFields = fields?.Cast<Field>()?.ToArray()?.Count(x => x.FieldState == EnumFieldState.Break) ?? int.MaxValue;
            if(breakableFields > 0)
                currentWaitTillShift = WAIT_TILL_SHIFT_TIME;

            if (currentWaitTillShift < 0 && IsRowUnlocked(rows - 2) && !isShifting || Input.GetKeyDown(KeyCode.A))
			{
                isShifting = true;

                Vector2 startPosition = fields[rows - 1, 0].fieldPosition + new Vector2(0, -fieldSize);
                Field[] newFields = new Field[8];

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
                        verticalMatch = fields[rows - 1, i].fieldVariant == fields[rows - 2, i].fieldVariant && fields[rows - 1, i].fieldVariant == fieldVariant;
                        horizontalMatch = 1 < i && newFields[i - 1].fieldVariant == newFields[i - 2].fieldVariant && newFields[i - 1].fieldVariant == fieldVariant;
                    } while (verticalMatch || horizontalMatch);

                    Field field = new Field(-1, -1, fieldVariant, 1, fieldPos, fieldUI);
                    fieldUI.transform.position = fieldPos;
                    fieldUI.Initialize(field);
                    fieldUI.ResetPosition();
                    fieldUI.SetLocked();

                    newFields[i] = field;
                }
                
                StartCoroutine(ShiftBoard(newFields));
            }
			else
			{
                currentWaitTillShift -= Time.deltaTime;
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
                    fields[rowIndex, columnIndex].fieldVariant = fields[rowIndex + 1, columnIndex].fieldVariant;
                    fields[rowIndex, columnIndex].fieldUI.Initialize(fields[rowIndex, columnIndex]);
                }
            }

            for (int columnIndex = 0; columnIndex < columns; columnIndex++)
			{
                fields[rows - 1, columnIndex].fieldVariant = newFields[columnIndex].fieldVariant;
                fields[rows - 1, columnIndex].fieldUI.Initialize(fields[rows - 1, columnIndex]);
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

        public void StartupReplaceBreakable()
        {
            int[] lvFieldType = new int[4];
            int lvTileCount = GM.boardMng.gameParameters.TileVariantMax();

            List<Mergeable> lvMergeables = FindBreakableFields();

            for (int i = 0; i < lvMergeables.Count; i++)
            {
                Field lvReplace = lvMergeables[i].GetReplaceableField();
                while (lvReplace != null)
                {
                    lvFieldType[0] = lvReplace.Left?.fieldVariant ?? lvReplace.fieldVariant;
                    lvFieldType[1] = lvReplace.Right?.fieldVariant ?? lvReplace.fieldVariant;
                    lvFieldType[2] = lvReplace.Top?.fieldVariant ?? lvReplace.fieldVariant;
                    lvFieldType[3] = lvReplace.Bottom?.fieldVariant ?? lvReplace.fieldVariant;

                    while (lvReplace.fieldVariant == lvFieldType[0] || lvReplace.fieldVariant == lvFieldType[1] || lvReplace.fieldVariant == lvFieldType[2] || lvReplace.fieldVariant == lvFieldType[3])
                        lvReplace.fieldVariant = GM.GetRandom(0, lvTileCount);

                    lvReplace = lvMergeables[i].GetReplaceableField();
                }
            }
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
                        lvSwapFieldTo = fields[_field.rowIndex, _field.columnIndex + 1];
                        lvIsRow = true;
                    }
                }
                else if (_swapDirection == EnumSwapDirection.Left)
                {
                    if (_field.columnIndex > 0)
                    {
                        lvSwapFieldTo = fields[_field.rowIndex, _field.columnIndex - 1];
                        lvSwapTopLeft = true;
                        lvIsRow = true;
                    }
                }
                else if (_swapDirection == EnumSwapDirection.Up)
                {
                    if (_field.rowIndex > 0)
                    {
                        lvSwapFieldTo = fields[_field.rowIndex - 1, _field.columnIndex];
                        lvSwapTopLeft = true;
                    }
                }
                else if (_swapDirection == EnumSwapDirection.Down)
                {
                    if (_field.rowIndex < rows - 1 && false == fields[_field.rowIndex + 1, _field.columnIndex].fieldUI.Locked)
					{
                        lvSwapFieldTo = fields[_field.rowIndex + 1, _field.columnIndex];
					}
                }

                if (lvSwapFieldTo != null && lvSwapFieldTo.FieldState == EnumFieldState.Useable)
                {
                    if (!_hintSwap)
                        _field.SwapWithField(lvSwapFieldTo);
                    else
                    {
                        Mergeable lvHintSwapMergeable = new Mergeable(2, lvIsRow, _field.fieldVariant);
                        if (lvIsRow)
                            lvHintSwapMergeable.breakUIWidth = new Vector2(2, 1);
                        else
                            lvHintSwapMergeable.breakUIWidth = new Vector2(1, 2);

                        if (!lvSwapTopLeft)
                        {
                            lvHintSwapMergeable.fields.Add(_field);
                            lvHintSwapMergeable.fields.Add(lvSwapFieldTo);
                            lvHintSwapMergeable.TopLeftField = _field;
                        }
                        else
                        {
                            lvHintSwapMergeable.fields.Add(lvSwapFieldTo);
                            lvHintSwapMergeable.fields.Add(_field);
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
                        if (_ignoreFieldState || fields[lvRowInxed, lvColumnIndex].FieldState == EnumFieldState.Useable && false == fields[lvRowInxed, lvColumnIndex].fieldUI.Locked)
                            lvStartIndex = lvRowInxed;
                    }
                    else
                    {
                        if (!_ignoreFieldState && 
                            ((fields[lvRowInxed, lvColumnIndex].FieldState != EnumFieldState.Useable) || fields[lvStartIndex, lvColumnIndex].fieldVariant != fields[lvRowInxed, lvColumnIndex].fieldVariant || fields[lvRowInxed, lvColumnIndex].fieldUI.Locked))
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
                            Mergeable lvMergeable = new Mergeable(lvEndIndex - lvStartIndex + 1, false, fields[lvStartIndex, lvColumnIndex].fieldVariant);
                            for (int i = lvStartIndex; i <= lvEndIndex; i++)
                                lvMergeable.fields.Add(fields[i, lvColumnIndex]);

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
                        if (_ignoreFieldState || fields[lvRowInxed, lvColumnIndex].FieldState == EnumFieldState.Useable && false == fields[lvRowInxed, lvColumnIndex].fieldUI.Locked)
                            lvStartIndex = lvColumnIndex;
                    }
                    else
                    {
                        if ((!_ignoreFieldState && 
                            (fields[lvRowInxed, lvColumnIndex].FieldState != EnumFieldState.Useable) || fields[lvRowInxed, lvStartIndex].fieldVariant != fields[lvRowInxed, lvColumnIndex].fieldVariant || fields[lvRowInxed, lvColumnIndex].fieldUI.Locked))
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
                            Mergeable lvMergeable = new Mergeable(lvEndIndex - lvStartIndex + 1, true, fields[lvRowInxed, lvStartIndex].fieldVariant);
                            for (int i = lvStartIndex; i <= lvEndIndex; i++)
                                lvMergeable.fields.Add(fields[lvRowInxed, i]);

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
                if (!lvResult[i].isRow && lvResult[i].fields.Count == minimumFieldCountForBreak)
                {
                    Field lvField = lvResult[i].fields[0];

                    for (int j = 0; j < ExactMatchDatas.Length; j++)
                    {
                        if (ExactMatchDatas[j].GetExactMatchingFields(lvField, lvExactMatchs))
                        {
                            Mergeable lvMergeable = new Mergeable(5, false, lvField.fieldVariant);
                            lvMergeable.fields.AddRange(lvExactMatchs);
                            lvMergeable.fields.AddRange(lvResult[i].fields);
                            lvMergeable.UpdateBoxFieldTo(lvResult[i].fields[ExactMatchDatas[j].swapIndex]);
                            lvNewExactMergeable.Add(lvMergeable);
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < lvResult.Count; i++)
            {
                if (lvResult[i].fields.Count == minimumFieldCountForBreak)
                {
                    for (int j = 0; j < lvNewExactMergeable.Count; j++)
                    {
                        if (lvNewExactMergeable[j].fields.Contains(lvResult[i].fields[0]) &&
                            lvNewExactMergeable[j].fields.Contains(lvResult[i].fields[1]) &&
                            lvNewExactMergeable[j].fields.Contains(lvResult[i].fields[2]))
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
                    int lvFieldType = fields[lvRows, lvColumns].fieldVariant;

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

                            if (fields[lvYDis, lvXDis].fieldVariant != lvFieldType)
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
                                OnSwapFields(fields[lvRows + lvDisp.y, lvColumns + lvDisp.x], MatchDatas[i].SwapDirection, true);
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
                if (swapField.Top != null && swapField.Top.fieldVariant == goodField.fieldVariant)
                    lvMergeable.possibleSwapData = new Mergeable.PossibleSwap(swapField, swapField.Top);
                else if (swapField.Bottom != null && swapField.Bottom.fieldVariant == goodField.fieldVariant)
                    lvMergeable.possibleSwapData = new Mergeable.PossibleSwap(swapField, swapField.Bottom);
                else if (goodFieldCount >= minimumFieldCountForBreak)
                    lvMergeable.possibleSwapData = new Mergeable.PossibleSwap(swapField, swapField.Left == startField ? startField : swapField.Right);
                else
                    lvMergeable = null;
            }
            else
            {
                if (swapField.Left != null && swapField.Left.fieldVariant == goodField.fieldVariant)
                    lvMergeable.possibleSwapData = new Mergeable.PossibleSwap(swapField, swapField.Left);
                else if (swapField.Right != null && swapField.Right.fieldVariant == goodField.fieldVariant)
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
                        lvMergeable.fields.Add(fields[startField.rowIndex, startField.columnIndex + i]);
                    else
                        lvMergeable.fields.Add(fields[startField.rowIndex + i, startField.columnIndex]);
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
                    swapField = fields[GM.GetRandom(0, rows), GM.GetRandom(0, columns)];

                    if (swapField != fields[lvRowIndex, lvColumnIndex] && swapField.fieldVariant != fields[lvRowIndex, lvColumnIndex].fieldVariant)
                        fields[lvRowIndex, lvColumnIndex].RandomSwap(swapField);
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
                            if (lvReplaceField.Top != null && lvReplaceField.Top.NeighborsWithFieldType(lvReplaceField.fieldVariant) == 1 && lvReplaceField.NeighborsWithFieldType(lvReplaceField.Top.fieldVariant) == 0)
                                lvReplaceField.RandomSwap(lvReplaceField.Top);
                            else if (lvReplaceField.Bottom != null && lvReplaceField.Bottom.NeighborsWithFieldType(lvReplaceField.fieldVariant) == 1 && lvReplaceField.NeighborsWithFieldType(lvReplaceField.Bottom.fieldVariant) == 0)
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
                            if (lvReplaceField.Left != null && lvReplaceField.Left.NeighborsWithFieldType(lvReplaceField.fieldVariant) == 1 && lvReplaceField.NeighborsWithFieldType(lvReplaceField.Left.fieldVariant) == 0)
                                lvReplaceField.RandomSwap(lvReplaceField.Left);
                            else if (lvReplaceField.Right != null && lvReplaceField.Right.NeighborsWithFieldType(lvReplaceField.fieldVariant) == 1 && lvReplaceField.NeighborsWithFieldType(lvReplaceField.Right.fieldVariant) == 0)
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
                lvResultField = fields[GM.GetRandom(0, rows), GM.GetRandom(0, columns)];
                if (lvResultField.NeighborsWithFieldType(_replaceField.fieldVariant) == 0 && _replaceField.NeighborsWithFieldType(lvResultField.fieldVariant) == 0)
                {
                    for (int i = 0; i < _mergeables.Count; i++)
                    {
                        if (_mergeables[i].fields.Contains(lvResultField))
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
                        if (fields[rowIndex, columnIndex].FieldState != EnumFieldState.Useable)
                            fields[rowIndex, columnIndex].ChangeFieldState(EnumFieldState.Useable);
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
                        int lvRowIndex = _mergeables[i].fields[0].rowIndex;

                        _mergeables[i].fields.Clear();

                        for (int j = 0; j < columns; j++)
                            _mergeables[i].fields.Add(fields[lvRowIndex, j]);

                        _mergeables[i].breakUIWidth = new Vector2(columns, 1);
                        _mergeables[i].TopLeftField = fields[lvRowIndex, 0];
                    }
                    else
                    {
                        int lvColumnIndex = _mergeables[i].fields[0].columnIndex;

                        _mergeables[i].fields.Clear();

                        for (int j = 0; j < rows; j++)
                            _mergeables[i].fields.Add(fields[j, lvColumnIndex]);

                        _mergeables[i].breakUIWidth = new Vector2(1, rows);
                        _mergeables[i].TopLeftField = fields[0, lvColumnIndex];
                    }
                }

                if (_mergeables[i].mergeableType == EnumMergeableType.Box)
                {
                    _mergeables[i].fields.Clear();

                    Vector2Int lvTopLeft = new Vector2Int(Mathf.Clamp(_mergeables[i].BoxField.columnIndex - 2, 0, columns - 1), Mathf.Clamp(_mergeables[i].BoxField.rowIndex - 2, 0, rows - 1));
                    Vector2Int lvBottomRight = new Vector2Int(Mathf.Clamp(_mergeables[i].BoxField.columnIndex + 2, 0, columns - 1), Mathf.Clamp(_mergeables[i].BoxField.rowIndex + 2, 0, rows - 1));

                    for (int row = lvTopLeft.y; row <= lvBottomRight.y; row++)
                    {
                        for (int column = lvTopLeft.x; column <= lvBottomRight.x; column++)
                            _mergeables[i].fields.Add(fields[row, column]);
                    }

                    _mergeables[i].breakUIWidth = lvBottomRight - lvTopLeft + Vector2.one;
                    _mergeables[i].TopLeftField = fields[lvTopLeft.y, lvTopLeft.x];
                }

                //Go backward to break the lowest tile first
                for (int j = _mergeables[i].fields.Count - 1; j >= 0; j--)
                {
                    if (_mergeables[i].TopLeftField == null)
                    {
                        if (_mergeables[i].isRow)
                            _mergeables[i].breakUIWidth = new Vector2(_mergeables[i].fields.Count, 1);
                        else
                            _mergeables[i].breakUIWidth = new Vector2(1, _mergeables[i].fields.Count);

                        _mergeables[i].TopLeftField = _mergeables[i].fields[0];
                    }

                    if (_mergeables[i].fields.Count == minimumFieldCountForBreak)
                        _mergeables[i].fields[j].Break(breakDelayTimeFast);
                    else
                        _mergeables[i].fields[j].Break(breakDelayTime);

                    UnlockFieldIfPossible(_mergeables[i].fields[j].Bottom);
                }

                _mergeables[i].PlayBreakSound();
            }
        }

		private void UnlockFieldIfPossible(Field bottom)
		{
            if (bottom != null && bottom.fieldUI.Locked)
                bottom.fieldUI.SetUnlockedIfNotUnbreakable();
        }

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

            foreach (var f in fields)
            {
                if (f.fieldVariant == targetField.Field.fieldVariant && false == f.fieldUI.Locked)
                    fieldsToBreak.Add(f);
            }

            BreakMultipleDelayed(fieldsToBreak);
        }

        private void HammerBreak(FieldUI targetField)
        {
            if(false == targetField.Locked)
                targetField.Field.Break(.5f);
        }

        private void ShovelBreak(FieldUI targetField)
        {
            List<Field> fieldsToBreak = new List<Field>();

            foreach (var f in fields)
            {
                if (f.rowIndex == targetField.Field.rowIndex && false == f.fieldUI.Locked)
                    fieldsToBreak.Add(f);
            }

            BreakMultipleDelayed(fieldsToBreak);
        }

        IEnumerator FireBreakRoutine(FieldUI targetField)
        {
            HashSet<Field> fieldsToBreak = new HashSet<Field>();

            foreach (var f in fields)
            {
                if (f.fieldVariant == targetField.Field.fieldVariant && false == f.fieldUI.Locked)
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
                    bool fieldOnHorizontalOppositeSideOnFire = f.rowIndex + 2 < rows && f.fieldUI.OnFire && fields[f.rowIndex + 2, f.columnIndex].fieldUI.OnFire;
                    bool fieldOnVerticalOppositeSideOnFire = f.columnIndex + 2 < columns && f.fieldUI.OnFire && fields[f.rowIndex, f.columnIndex + 2].fieldUI.OnFire;
                    bool fieldOnDiagonalOppositeSideOnFire = f.rowIndex + 2 < rows && f.columnIndex + 2 < columns && f.fieldUI.OnFire && fields[f.rowIndex + 2, f.columnIndex + 2].fieldUI.OnFire;

                    if (fieldOnHorizontalOppositeSideOnFire && false == fields[f.rowIndex + 1, f.columnIndex].fieldUI.Locked)
                        newFieldsOnFire.Add(fields[f.rowIndex + 1, f.columnIndex]);
                    if (fieldOnVerticalOppositeSideOnFire && false == fields[f.rowIndex, f.columnIndex + 1].fieldUI.Locked)
                        newFieldsOnFire.Add(fields[f.rowIndex, f.columnIndex + 1]);
                    if (fieldOnDiagonalOppositeSideOnFire && false == fields[f.rowIndex + 1, f.columnIndex + 1].fieldUI.Locked)
                        newFieldsOnFire.Add(fields[f.rowIndex + 1, f.columnIndex + 1]);
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

                    if(false == fields[currentFieldPos.x, currentFieldPos.y].fieldUI.Locked)
                        fieldsToBreak.Add(fields[currentFieldPos.x, currentFieldPos.y]);
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
                if (lvRowIndex == rows - 1 && fields[lvRowIndex, _columnIndex].FieldState == EnumFieldState.Empty && !_initCall) {
                    columnTopRow[_columnIndex] = 0;
                    //Debug.Log("index: " + _columnIndex + ", value: " + columnTopRow[_columnIndex]);
                }

                Field lvFieldAbove = null;

                //feed column
                if (fields[lvRowIndex, _columnIndex].FieldState == EnumFieldState.Empty)
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
                        if (fields[lvRowAbove, _columnIndex].FieldState == EnumFieldState.Useable || fields[lvRowAbove, _columnIndex].FieldState == EnumFieldState.Move || fields[lvRowAbove, _columnIndex].FieldState == EnumFieldState.ComboReady)
                        {
                            lvFieldAbove = fields[lvRowAbove, _columnIndex];
                            fields[lvRowIndex, _columnIndex].MoveFieldHere(lvFieldAbove);
                        }
                        else if (fields[lvRowAbove, _columnIndex].FieldState != EnumFieldState.Empty && fields[lvRowAbove, _columnIndex].FieldState != EnumFieldState.Hidden)
                            lvFieldAbove = fields[lvRowAbove, _columnIndex];
                    }

                    //Add new tile to top
                    if (lvFieldAbove == null)
                    {
                        if (endGame)
                            continue;

                        lvFirstRefillField++;
                        fields[lvRowIndex, _columnIndex].ChangeFieldState(EnumFieldState.Move);
                        fields[lvRowIndex, _columnIndex].fieldUI.ResetPositionToRefil(lvFirstRefillField);
                        fields[lvRowIndex, _columnIndex].fieldVariant = columnFeeds[_columnIndex].GetFieldType(fields[lvRowIndex, _columnIndex].fieldUI);
                        fields[lvRowIndex, _columnIndex].fieldUI.Initialize(fields[lvRowIndex, _columnIndex]);
                        fields[lvRowIndex, _columnIndex].fieldUI.gameObject.SetActive(true);
                    }
                }
            }
        }


        bool IsRowUnlocked(int row)
		{
            for (int i = 0; i < columns; ++i)
			{
                if (fields[row, i].fieldUI.Locked)
                    return false;
			}

            return true;
        }

        void SetRowLocked(int row)
		{
            for (int i = 0; i < columns; ++i)
                fields[row, i].fieldUI.SetLocked();
		}

        private void SetRowUnBreakAble(int row)
        {
            for (int i = 0; i < columns; ++i)
                fields[row, i].fieldUI.SetUnbreakableAndLocked();
        }

        // public void AddCoinToBoard(Field _onField)
        // {
        //     if (_onField != null)
        //         columnFeeds[_onField.columnIndex].AddField(gameParameters.TileVariantMax());
        //     else
        //     {
        //         lastComboBreakColumn++;
        //         lastComboBreakColumn = lastComboBreakColumn == columns ? 0 : lastComboBreakColumn;

        //         Field lvFirstBreakField = null;

        //         for (int lvColumnIndex = lastComboBreakColumn; lvColumnIndex < columns && lvFirstBreakField == null; lvColumnIndex++)
        //         {
        //             if (!columnFeeds[lvColumnIndex].HasCoin())
        //             {
        //                 for (int rowIndex = 0; rowIndex < rows && lvFirstBreakField == null; rowIndex++)
        //                 {
        //                     if (fields[rowIndex, lvColumnIndex].FieldState == EnumFieldState.Break)
        //                         lvFirstBreakField = fields[rowIndex, lvColumnIndex];
        //                 }
        //             }

        //         }

        //         for (int lvColumnIndex = 0; lvColumnIndex < lastComboBreakColumn && lvFirstBreakField == null; lvColumnIndex++)
        //         {
        //             if (!columnFeeds[lvColumnIndex].HasCoin())
        //             {
        //                 for (int rowIndex = 0; rowIndex < rows && lvFirstBreakField == null; rowIndex++)
        //                 {
        //                     if (fields[rowIndex, lvColumnIndex].FieldState == EnumFieldState.Break)
        //                         lvFirstBreakField = fields[rowIndex, lvColumnIndex];
        //                 }
        //             }
        //         }

        //         if (lvFirstBreakField != null)
        //             columnFeeds[lvFirstBreakField.columnIndex].AddField(gameParameters.TileVariantMax());
        //         else
        //             columnFeeds[lastComboBreakColumn].AddField(gameParameters.TileVariantMax());
        //     }
        // }

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
                        lvMergeables[0].fields.Add(fields[lvRowIndex, lvColumnIndex]);

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

    [System.Serializable]
    public class ColumnFeed
    {
        public int column;
        public List<int> fieldTypes;

        System.Random seed = new System.Random(GM.GetRandom(0, int.MaxValue));

        public ColumnFeed(int _column, List<int> _colors = null)
        {
            column = _column;
            fieldTypes = new List<int>();

            if (_colors != null)
                fieldTypes.AddRange(_colors);
        }

        public int GetFieldType(FieldUI _fieldUI)
        {
            if (fieldTypes.Count > 0)
            {
                int lvResult = fieldTypes[0];

                fieldTypes.RemoveAt(0);

                return lvResult;
            }
            else
                return seed.Next() % GM.boardMng.gameParameters.TileVariantMax();
        }

        public void AddField(int _fieldType)
        {
            fieldTypes.Add(_fieldType);
        }

        // public bool HasCoin()
        // {
        //     return fieldTypes.Count > 0 && fieldTypes.Contains(GM.boardMng.gameParameters.TileVariantMax());
        // }
    }

    [Serializable]
    public class FieldData
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

                if (GM.boardMng.Fields[lvYDis, lvXDis].fieldVariant != _matchTo.fieldVariant)
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
}