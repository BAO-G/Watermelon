#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace WatermelonEditor
{
    public static class WatermelonSceneSetup
    {
        private const string StartPanelName = "StartPanel";
        private const string GamePanelName = "GamePanel";
        private const string GameOverPanelName = "GameOverPanel";

        [MenuItem("Watermelon/Setup Scene", false, 0)]
        public static void SetupScene()
        {
            Undo.SetCurrentGroupName("Setup Watermelon Scene");
            int group = Undo.GetCurrentGroup();

            Canvas canvas = EnsureCanvas();
            EnsureEventSystem();
            ConfigureCamera();

            GameObject managersObj = EnsureManagers();
            GameManager gameManager = managersObj.GetComponent<GameManager>();
            UIManager uiManager = managersObj.GetComponent<UIManager>();

            GameObject startPanel = EnsurePanel(canvas.transform, StartPanelName);
            GameObject gamePanel = EnsurePanel(canvas.transform, GamePanelName);
            GameObject gameOverPanel = EnsurePanel(canvas.transform, GameOverPanelName);

            SetupStartPanel(startPanel, gameManager, uiManager);
            SetupGamePanel(gamePanel, uiManager);
            SetupGameOverPanel(gameOverPanel, gameManager, uiManager);

            uiManager.startPanel = startPanel;
            uiManager.gamePanel = gamePanel;
            uiManager.gameOverPanel = gameOverPanel;

            DisableOldButton(canvas.transform);

            EditorUtility.SetDirty(gameManager);
            EditorUtility.SetDirty(uiManager);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Undo.CollapseUndoOperations(group);
            Debug.Log("[WatermelonSceneSetup] 场景设置完成。请在 Inspector 中检查 GameManager 的水果 Prefab 引用。");
        }

        private const string TextureFolder = "Assets/Texture/";

        private static Canvas EnsureCanvas()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0f;
            }

            // 添加游戏背景（黄色背景图）
            EnsureCanvasBackground(canvas);

            return canvas;
        }

        private static void EnsureCanvasBackground(Canvas canvas)
        {
            Transform bgTransform = canvas.transform.Find("CanvasBackground");
            GameObject bgObj;

            if (bgTransform != null)
            {
                bgObj = bgTransform.gameObject;
            }
            else
            {
                bgObj = new GameObject("CanvasBackground", typeof(RectTransform), typeof(Image));
                bgObj.transform.SetParent(canvas.transform, false);
                bgObj.transform.SetAsFirstSibling();
            }

            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                TextureFolder + "856267d0-6891-4660-a28a-3eb110bf6395.png");
            Image bgImage = bgObj.GetComponent<Image>();
            if (bgSprite != null)
            {
                bgImage.sprite = bgSprite;
                bgImage.color = Color.white;
            }
            else
            {
                bgImage.color = new Color(0.95f, 0.85f, 0.6f);
            }
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
            }
        }

        private static void ConfigureCamera()
        {
            Camera camera = Camera.main;
            if (camera != null)
            {
                // orthographicSize 设为 6.5，确保完整显示游戏区域
                // 可视范围: 垂直 y=-6.5 到 y=6.5，水平宽度随屏幕比例自适应
                // 配合 GameManager 的 leftBound(-3.3) / rightBound(3.3) 和墙壁碰撞体使用
                camera.orthographicSize = 6.5f;
                EditorUtility.SetDirty(camera);
            }
        }

        private static GameObject EnsureManagers()
        {
            GameManager existingGameManager = Object.FindObjectOfType<GameManager>();
            GameObject managersObj;

            if (existingGameManager != null)
            {
                managersObj = existingGameManager.gameObject;
            }
            else
            {
                managersObj = GameObject.Find("Managers");
                if (managersObj == null)
                {
                    managersObj = new GameObject("Managers");
                    Undo.RegisterCreatedObjectUndo(managersObj, "Create Managers");
                }
                managersObj.AddComponent<GameManager>();
            }

            EnsureComponent<UIManager>(managersObj);
            EnsureComponent<AudioManager>(managersObj);
            EnsureComponent<EffectsManager>(managersObj);
            EnsureComponent<ScorePopupManager>(managersObj);
            EnsureComponent<VibrationManager>(managersObj);

            return managersObj;
        }

        private static void EnsureComponent<T>(GameObject obj) where T : MonoBehaviour
        {
            if (obj.GetComponent<T>() == null)
            {
                obj.AddComponent<T>();
            }
        }

        private static GameObject EnsurePanel(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image image = panel.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.75f);

            Undo.RegisterCreatedObjectUndo(panel, "Create Panel " + name);
            return panel;
        }

        private static void SetupStartPanel(GameObject panel, GameManager gameManager, UIManager uiManager)
        {
            ClearChildren(panel.transform);

            // 完整西瓜图标（装饰）
            Sprite watermelonSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                TextureFolder + "5035266c-8df3-4236-8d82-a375e97a0d9c.png");
            if (watermelonSprite != null)
            {
                GameObject iconObj = new GameObject("WatermelonIcon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(panel.transform, false);
                Image iconImage = iconObj.GetComponent<Image>();
                iconImage.sprite = watermelonSprite;
                iconImage.preserveAspect = true;
                RectTransform iconRt = iconObj.GetComponent<RectTransform>();
                iconRt.anchoredPosition = new Vector2(0, 520);
                iconRt.sizeDelta = new Vector2(180, 180);
            }

            Text title = CreateText(panel.transform, "TitleText", "合成大西瓜", 96);
            title.rectTransform.anchoredPosition = new Vector2(0, 380);
            title.rectTransform.sizeDelta = new Vector2(600, 140);

            Text highScore = CreateText(panel.transform, "HighScoreText", "最高分: 0", 48);
            highScore.rectTransform.anchoredPosition = new Vector2(0, 200);
            highScore.rectTransform.sizeDelta = new Vector2(500, 80);

            Button startButton = CreateButton(panel.transform, "StartButton", "开始游戏");
            startButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -100);

            // ★ 修复：不再使用 preserveAspect 的圆形 sprite 作为按钮背景
            // preserveAspect 会导致正方形/圆形图在矩形按钮中缩小时遮挡文字
            // 改为纯色填充背景，文字清晰可见
            Sprite buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                TextureFolder + "a7de1099-ffab-450b-8db5-54b51514fd54.png");
            if (buttonSprite != null)
            {
                Image btnImage = startButton.GetComponent<Image>();
                btnImage.sprite = buttonSprite;
                btnImage.type = Image.Type.Sliced;
                btnImage.preserveAspect = false;  // 拉伸填充，不保留原始比例
            }

            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(gameManager.StartGame);

            gameManager.startButton = startButton;

            // 绑定标题文字引用，供 UIManager 播放呼吸动画
            uiManager.startTitleText = title;
        }

        private static void SetupGamePanel(GameObject panel, UIManager uiManager)
        {
            ClearChildren(panel.transform);

            Image bg = panel.GetComponent<Image>();
            bg.color = new Color(0, 0, 0, 0);

            Text score = CreateText(panel.transform, "ScoreText", "0", 72);
            score.rectTransform.anchorMin = new Vector2(0, 1);
            score.rectTransform.anchorMax = new Vector2(0, 1);
            score.rectTransform.pivot = new Vector2(0, 1);
            score.rectTransform.anchoredPosition = new Vector2(50, -50);
            score.rectTransform.sizeDelta = new Vector2(400, 100);

            Text highScore = CreateText(panel.transform, "HighScoreText", "最高分: 0", 48);
            highScore.rectTransform.anchorMin = new Vector2(1, 1);
            highScore.rectTransform.anchorMax = new Vector2(1, 1);
            highScore.rectTransform.pivot = new Vector2(1, 1);
            highScore.rectTransform.anchoredPosition = new Vector2(-50, -50);
            highScore.rectTransform.sizeDelta = new Vector2(400, 80);

            Text previewLabel = CreateText(panel.transform, "NextLabel", "下一个", 36);
            previewLabel.rectTransform.anchorMin = new Vector2(1, 1);
            previewLabel.rectTransform.anchorMax = new Vector2(1, 1);
            previewLabel.rectTransform.pivot = new Vector2(1, 1);
            previewLabel.rectTransform.anchoredPosition = new Vector2(-50, -140);
            previewLabel.rectTransform.sizeDelta = new Vector2(150, 50);

            GameObject previewObj = new GameObject("NextFruitPreview", typeof(RectTransform), typeof(Image));
            previewObj.transform.SetParent(panel.transform, false);
            RectTransform previewRt = previewObj.GetComponent<RectTransform>();
            previewRt.anchorMin = new Vector2(1, 1);
            previewRt.anchorMax = new Vector2(1, 1);
            previewRt.pivot = new Vector2(1, 1);
            previewRt.anchoredPosition = new Vector2(-50, -200);
            previewRt.sizeDelta = new Vector2(120, 120);
            Image previewImage = previewObj.GetComponent<Image>();
            previewImage.preserveAspect = true;

            uiManager.scoreText = score;
            uiManager.highScoreText = highScore;
            uiManager.nextFruitImage = previewImage;
        }

        private static void SetupGameOverPanel(GameObject panel, GameManager gameManager, UIManager uiManager)
        {
            ClearChildren(panel.transform);

            Text title = CreateText(panel.transform, "TitleText", "游戏结束", 84);
            title.rectTransform.anchoredPosition = new Vector2(0, 300);
            title.rectTransform.sizeDelta = new Vector2(500, 120);

            Text finalScore = CreateText(panel.transform, "FinalScoreText", "最终分数: 0", 60);
            finalScore.rectTransform.anchoredPosition = new Vector2(0, 100);
            finalScore.rectTransform.sizeDelta = new Vector2(600, 100);

            Text newRecord = CreateText(panel.transform, "NewRecordText", "新纪录!", 60);
            newRecord.rectTransform.anchoredPosition = new Vector2(0, -50);
            newRecord.rectTransform.sizeDelta = new Vector2(400, 100);
            newRecord.color = Color.yellow;
            newRecord.gameObject.SetActive(false);

            Button restartButton = CreateButton(panel.transform, "RestartButton", "重新开始");
            restartButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -250);
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(gameManager.RestartGame);

            gameManager.restartButton = restartButton;
            uiManager.finalScoreText = finalScore;
            uiManager.newRecordText = newRecord;
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Shadow));
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.GetComponent<RectTransform>();

            // ★ 关键修复：必须显式设置锚点和轴心，否则 anchoredPosition 和 sizeDelta 行为异常
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            Text text = obj.GetComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 防止文字在不同分辨率和容器尺寸下被截断
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            Shadow shadow = obj.GetComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(2, -2);

            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 120);

            // ★ 关键修复：设置锚点和轴心，确保位置计算正确
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            Image image = obj.GetComponent<Image>();
            image.color = new Color(1, 0.5f, 0.6f);

            Text text = CreateText(obj.transform, "Text", label, 48);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;

            // 添加按钮交互动画组件
            UIButtonAnimator btnAnim = obj.AddComponent<UIButtonAnimator>();

            return obj.GetComponent<Button>();
        }

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);
            }
        }

        private static void DisableOldButton(Transform canvas)
        {
            Transform oldButton = canvas.Find("Button");
            if (oldButton != null)
            {
                GameObject buttonObj = oldButton.gameObject;
                buttonObj.SetActive(false);
                EditorUtility.SetDirty(buttonObj);
            }
        }
    }
}
#endif
