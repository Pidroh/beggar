using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeartUnity.View
{

    public partial class UIUnit : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
    {
        public static EngineView EngineView;
        public InputData InputData = new InputData();
        private RectTransform _rectTransform;

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }
                return _rectTransform;
            }
        }
        public bool Clicked
        {
            get
            {
                var inputEnabled = true;
                var inputManagerExist = EngineView != null && EngineView.inputManager != null;
                if (inputManagerExist) inputEnabled = EngineView.inputManager.InputEnabled;
                if (HoveredWhileVisible && inputManagerExist) 
                {
                    EngineView?.inputManager?.HoveredClickableThisFrame();
                }
                return (_clickedInternal || _longPressClick) && inputEnabled;
            }
        }

        public bool Pressed => _pressedInternal;
        public FontGroup fontGroup;
        public float fontSizeScale = 1f;

        public bool ButtonEnabled
        {
            get => _button.interactable;
            set => _button.interactable = value;
        }

        public float MouseDownTime = 0f;
        private float[] MouseDownTimeThresholds = { 1f, 2.0f, 3.5f, 4.5f };
        private float[] LongPressClickPeriods = { 0.3f, 0.2f, 0.1f, 0.01f };
        public bool LongPressMulticlickEnabled = false;

        private float longPressMultiClickCounter = 0f;
        private float currentLongPressClickPeriod = 0.4f;
        private bool _longPressClick = false;
        public bool LongPressClickHappenedThisFrame => _longPressClick;
        public Color? NormalColor;

        public void ConsumeClick()
        {
            _longPressClick = false;
            _clickedInternal = false;
        }

        public Color? ClickColor;


        public bool LongPress => MouseDownTime > 0.4f;

        private bool _hoveredWhileVisible;

        private bool _pressedInternal;
        private bool _clickedInternal;
        public bool MouseDown;

        public bool MouseUpThisFrame;

        public bool MouseDownThisFrame;
        public TextMeshProUGUI text;
        public int lastNumber = -123892018;
        public Movement movement;
        private bool _inited;
        private Image _image;
        public Vector3 originalPosition;
        public GameObject selectedImage;

        public void Init()
        {
            if (_inited) return;
            _inited = true;
            if (movement == null) movement = new Movement();
            movement.UiUnit = this;
            _rectTransform = _rectTransform == null ? GetComponent<RectTransform>() : _rectTransform;
            originalPosition = transform.position;
            if (TryGetComponent<Button>(out var button))
            {
                button.onClick.AddListener(OnClicked);
                _button = button;
            }
            if (text == null)
                text = GetComponent<TextMeshProUGUI>();
            if (_image == null) _image = GetComponent<Image>();
            UpdateFont();
            originalSprite = _image?.sprite;
            Selected = false;
        }

        public bool Selected
        {
            get => selectedImage != null && selectedImage.activeSelf; set
            {
                if (selectedImage == null) return;
                Init();
                selectedImage.SetActive(value);
            }
        }

        public void ForceClick()
        {
            _clickedInternal = true;
        }

        public void ChangeSprite(Sprite mainSprite, bool changeSize = true, bool updateOriginal = true)
        {
            Init();
            _image.sprite = mainSprite;
            originalSprite = mainSprite;
            if (changeSize)
                _image.rectTransform.sizeDelta = new Vector2(mainSprite.rect.width, mainSprite.rect.height);
        }

        public void ChangeSprite(Sprite sprite, float minWidth, float minHeight)
        {
            ChangeSprite(sprite);
            var sizeXSc = minWidth / sprite.rect.width;
            var sizeYSc = minHeight / sprite.rect.height;
            var scale = Mathf.Max(sizeXSc, sizeYSc);
            _image.rectTransform.sizeDelta = new Vector2(scale * sprite.rect.width, scale * sprite.rect.height);
        }

        public void ChangeSprite(SpriteInfo spriteInfo)
        {
            ChangeSprite(spriteInfo.sprite);
            transform.localScale = new Vector3(spriteInfo.scale, spriteInfo.scale, spriteInfo.scale);
            transform.localPosition = transform.localPosition + spriteInfo.offset;
        }



        public void SetTrasparency(float v)
        {
            Init();
            if (_image != null)
            {
                var c = _image.color;
                c.a = v;
                _image.color = c;
            }
            if (text != null)
            {
                var c = text.color;
                c.a = v;
                text.color = c;
            }
        }

        private Button _button;
        private Sprite originalSprite;

        

        public void SetTextRelatedToNumber(string v, int level)
        {
            text.text = v;
            lastNumber = level;
        }

        public void Process()
        {
            var position = RectTransform.transform.position;
        }

        private void OnClicked()
        {
            Debug.Log("DSADSAd");
            _clickedInternal = true;
        }

        public void EndFrame()
        {

        }

        void Awake()
        {
            Init();
        }

        public void AddNumberToText(int number)
        {
            NumberToText(number + lastNumber);
        }

        public void Show(bool v)
        {
            gameObject.SetActive(v);
        }

        public void ChangeHeightToFitTextPreferredHeight()
        {
            RectTransform.SetHeight(text.preferredHeight);
        }

        // Start is called before the first frame update
        void Start()
        {

        }


        public string rawText
        {
            get { return text.text; }
            set { text.text = value; }
        }

        public bool Active { get => gameObject.activeInHierarchy; set => this.gameObject.SetActive(value); }
        public bool ActiveSelf { get => gameObject.activeSelf; set => this.gameObject.SetActive(value); }
        public Vector3 OffsetFromOriginal
        {
            get
            {
                Vector3 value = transform.position - originalPosition;
                value.x /= this.transform.parent.lossyScale.x;
                value.y /= this.transform.parent.lossyScale.y;
                value.z /= this.transform.parent.lossyScale.z;
                return value;
            }
            set
            {

                value.x *= this.transform.parent.lossyScale.x;
                value.y *= this.transform.parent.lossyScale.y;
                value.z *= this.transform.parent.lossyScale.z;
                transform.position = originalPosition + value;
            }
        }

        public bool HoveredWhileVisible => _hoveredWhileVisible && gameObject.activeSelf;

        public Image Image { get => _image; set => _image = value; }
        public bool HasButton => _button != null;

        public int? FontSizePhysical;

        public bool CheckMouseInside()
        {
            return RectTransformUtility.RectangleContainsScreenPoint(RectTransform, InputWrapper.mousePosition, Camera.main);
            var yourRect = RectTransform;
            var position = yourRect.transform.position;
            var size = yourRect.sizeDelta;
            var diff = yourRect.pivot.x * size.x;
        }   

        public void NumberToText(int number)
        {
            if (number != lastNumber)
            {
                text.text = number + "";
                lastNumber = number;
            }
            if (number != lastNumber)
            {
                text.text = number + "";
                lastNumber = number;
            }
        }

        public void ShakeText()
        {
            text.rectTransform.DOKill();
            ToOriginalPosition();
            text.rectTransform.DOShakePosition(0.3f, new Vector3(10, 10), 40);
        }

        public void Shake()
        {
            transform.DOKill();
            ToOriginalPosition();
            transform.DOShakePosition(0.3f, new Vector3(10, 10), 40);
        }

        // Update is called once per frame
        void Update()
        {
            UpdateFont();
            movement.Update();
            _longPressClick = false;
            if (MouseDown)
            {
                MouseDownTime += Time.deltaTime;

                if (LongPressMulticlickEnabled && _button.interactable)
                {
                    for (int i = 0; i < MouseDownTimeThresholds.Length; i++)
                    {
                        if (MouseDownTime < MouseDownTimeThresholds[i])
                        {
                            currentLongPressClickPeriod = LongPressClickPeriods[i];
                            break;
                        }
                    }

                    longPressMultiClickCounter += Time.deltaTime;
                    if (longPressMultiClickCounter >= currentLongPressClickPeriod)
                    {
                        _longPressClick = true;
                        longPressMultiClickCounter = 0f;
                    }
                }
            }
            else
            {
                MouseDownTime = 0f;
                longPressMultiClickCounter = 0f;
            }
        }

        

        private void UpdateFont()
        {
            if (fontGroup != null && Local.HasMoreThaOneLanguage && text != null)
            {
                var fh = fontGroup.GetFont(Local.Instance.Lang.languageName);
                if (text.font != fh.fontAsset)
                {
                    if (fontSizeScale > 0)
                        text.fontSize = (int)fh.targetSize * fontSizeScale;
                    text.font = fh.fontAsset;
                }
            }
            if (FontSizePhysical.HasValue) {
                text.SetFontSizePhysical(FontSizePhysical.Value);
            }
        }

        public void ToOriginalPosition()
        {
            transform.position = originalPosition;
        }

        private void LateUpdate()
        {
            var clickedLastFrame = Clicked;
            if (clickedLastFrame && this.ClickColor.HasValue) 
            {
                _image.color = ClickColor.Value;
            }
            if (!clickedLastFrame && this.NormalColor.HasValue) 
            {
                _image.color = NormalColor.Value;
            }
            MouseDownThisFrame = false;
            MouseUpThisFrame = false;
            _clickedInternal = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hoveredWhileVisible = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hoveredWhileVisible = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            MouseDown = false;
            MouseUpThisFrame = true;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            MouseDown = true;
            MouseDownThisFrame = true;
        }

        public void SetTextKey(string v)
        {
            text.text = Local.GetText(v);
        }

        public void SetTextRaw(string v)
        {
            text.text = v;
        }

        private void OnDisable()
        {
            _clickedInternal = false;
        }

        public UIUnit SetTextAlignment(TextAlignmentOptions left)
        {
            text.alignment = left;
            return this;
        }

        public UIUnit SetParent(Transform parentRectTransform)
        {
            transform.SetParent(parentRectTransform);
            return this;
        }

        public UIUnit SetParent(UIUnit parent)
        {
            return SetParent(parent.RectTransform);
        }
    }

    public class InputData
    {
        public string dataText;
        public int data1 = -1;
        public int data2 = -1;
        public int data3 = -1;
        public GameObject gameObject;
    }
}