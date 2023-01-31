using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class UIManager : MonoBehaviour
{
    //07.18. BellEmpty
    //임시수치. 이후에 수정해야할 변수들
    public static UIManager Instance;

    #region UI Object
    public GameObject _playerInfoView = null;

    [Header("상태바")]
    public GameObject _hPBar = null;
    public GameObject _sPBar = null;
    public Image _hPBarImage = null;
    public Image _sPBarImage = null;

    [Header("탄약 슬롯")]
    public Button[] _ammoBtn = new Button[ConstNums.numberOfItemAmmo];
    public Image[] _ammoImg = new Image[ConstNums.numberOfItemAmmo];
    public TextMeshProUGUI[] _ammoTxt = new TextMeshProUGUI[ConstNums.numberOfItemAmmo];

    [Header("무기 슬롯")]
    public Button[] _weaponBtn = new Button[ConstNums.numberOfPlayerSlot];
    public Image[] _weaponImg = new Image[ConstNums.numberOfPlayerSlot];
    public Image[] _weaponCoolImg = new Image[ConstNums.numberOfPlayerSlot];
    public Image[] _weaponSelectBox = new Image[ConstNums.numberOfPlayerSlot];
    public TextMeshProUGUI[] _weaponTxt = new TextMeshProUGUI[ConstNums.numberOfPlayerSlot];
    public TextMeshProUGUI _throwWeaponCount = new TextMeshProUGUI();

    [Header("현재 탄약 슬롯")]
    public GameObject _curAmmoView = null;
    public Image[] _curAmmoImg = new Image[_curAmmoViewNum];
    public TextMeshProUGUI[] _curAmmoTxt = new TextMeshProUGUI[_curAmmoViewNum];

    [Header("방어구 슬롯")]
    public GameObject _defView = null;
    public Button[] _defBtn = new Button[ConstNums.numberOfItemDefensive];
    public Image[] _defImg = new Image[ConstNums.numberOfItemDefensive];
    public TextMeshProUGUI[] _defTxt = new TextMeshProUGUI[ConstNums.numberOfItemDefensive];

    [Header("회복류 슬롯")]
    public GameObject _recoverView = null;
    public Button[] _recoverBtn = new Button[ConstNums.numberOfItemRecovery];
    public Image[] _recoverImg = new Image[ConstNums.numberOfItemRecovery];
    public TextMeshProUGUI[] _recoverTxt = new TextMeshProUGUI[ConstNums.numberOfItemRecovery];

    [Header("배율 버튼")]
    public GameObject _scopeView = null;
    public Button[] _scopeBtn = new Button[ConstNums.numberOfItemScope];
    public TextMeshProUGUI[] _scopeTxt = new TextMeshProUGUI[ConstNums.numberOfItemScope];

    [Header("미니맵 슬롯")]
    public GameObject _minimapMask = null;
    public GameObject _bigmapMask = null;

    [Header("설명")]
    public GameObject _notiImage = null;

    [Header("네트워크 텍스트")]
    public TMP_Text _aliveTxt = null;
    public TMP_Text _timeTxt = null;
    public GameObject _playerCountView = null;
    public GameObject _timeView = null;
    public GameObject _topLogView = null;
    public GameObject _botLogView = null;
    public GameObject _guideView = null;
    public GameObject _topKillPrefab = null;
    public GameObject _botKillPrefab = null;
    public GameObject _guidePrefab = null;

    [Header("알림창")]
    public GameObject _pressFView = null;
    public Image _pressFImg = null;
    public Image _pressFNameImg = null;
    public TextMeshProUGUI _pressFNameTxt = null;
    public GameObject _actionView = null;
    public Image _actionImg = null;
    public Image _actionBackgroundImg = null;
    public TextMeshProUGUI _actionTxt = null;
    public GameObject _informView = null;
    public GameObject _informPrefab = null;

    [Header("엔딩")]
    public GameObject _endingView = null;
    public TMP_Text _endingTxt = null;
    public TMP_Text _myRankingTxt = null;
    public TMP_Text _ranking = null;
    public TMP_Text _rankingTxt = null;
    public TMP_Text _killCount = null;
    public TMP_Text _killCountTxt = null;
    public GameObject _watchingButton = null;
    public GameObject _leaveButton = null;
    public GameObject _watchingView = null;

    public GameObject cursor = null;
    #endregion

    private GraphicRaycaster graphicRaycaster = null;
    private PointerEventData pointerEventData = null;

    private bool _isDead = false;
    private int _curSlot = 0;
    private static Player _player = null;
    private static PlayerInfo _playerInfo = null;
    private bool miniMapOrBigMap = false;
    private bool notiOn = false;


    #region LocalConstGroup
    private const int _scopeConvert = 4;
    private const int _smallFont = 18;
    private const int _bigFont = 36;
    private Vector2 _smallScopeBtnSize = new Vector2(50, 50);
    private Vector2 _bigScopeBtnSize = new Vector2(100, 100);
    private const int _curAmmoViewNum = 2;
    private const float _maxHPRatio = 1.0f;
    private const float _redHPRatio = 0.7f;
    #endregion

    #region ColorGroup
    private Color32 _noneItemBackgroundColor = new Color32(0, 0, 0, 60);
    private Color32 _noneItemImageColor = new Color32(255, 255, 255, 60);
    private Color32 _alphaZeroColor = new Color32(255, 255, 255, 0);
    private Color32 _alphaOnColor = new Color32(0, 0, 0, 200);
    private Color32 _greyColor = new Color32(170, 170, 170, 255);
    private Color32 _whiteColor = Color.white;
    private Color32 _redHalfAlphaColor = new Color32(255, 50, 0, 200);
    private Color32 _oragngeColor = new Color32(255, 120, 0, 255);
    private Color32 _clearColor = Color.clear;
    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.Log("HUD Instance isn't NULL");
        }

        BindHPBar();
        BindSPBar();
        BindAmmoSlot();
        BindWeaponSlot();
        BindCurAmmoSlot();
        BindDefSlot();
        BindRecoverySlot();
        BindScopeSlot();
        BindNotiWindowView();
        ChangeCurSlot(ConstNums.subWeaponIndex);
        BindMap();
        BindNetworkView();
        BindEndingView();
        FindGameObject(ref _playerInfoView, "PlayerInfoView", true);

        graphicRaycaster = GetComponent<GraphicRaycaster>();
        pointerEventData = new PointerEventData(null);
    }

    private void Start()
    {
        ClickEventMatching();

        miniMapOrBigMap = false;
        notiOn = true;
    }

    private void Update()
    {
        //우클릭 Item Drop 기능
        if (Input.GetMouseButtonDown(1))
        {
            FindClickedButton();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _minimapMask.SetActive(true);
            _bigmapMask.SetActive(false);
            _timeView.SetActive(true);
            miniMapOrBigMap = false;

            _notiImage.SetActive(false);
            notiOn = false;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            if (miniMapOrBigMap == false)
            {
                _minimapMask.SetActive(false);
                _bigmapMask.SetActive(true);
                _timeView.SetActive(false);
                miniMapOrBigMap = true;
            }
            else
            {
                _minimapMask.SetActive(true);
                _bigmapMask.SetActive(false);
                _timeView.SetActive(true);
                miniMapOrBigMap = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (notiOn == true)
            {
                _notiImage.SetActive(false);
                notiOn = false;
            }
            else
            {
                _notiImage.SetActive(true);
                notiOn = true;
            }
        }

        UpdateCursor();

    }

    private void FindClickedButton()
    {
        pointerEventData.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, raycastResults);

        if (raycastResults.Count > 0)
        {
            UIButton uiBtn = null;
            Debug.Log(raycastResults[0]);
            if (raycastResults[0].gameObject.TryGetComponent<UIButton>(out uiBtn))
            {
                //우클릭 대상 버튼 정보 전달
                CallDropItemByItemType(uiBtn);
            }
            else
            {
                Debug.Log("It's not a Button");
            }
        }
    }

    #region BindUI
    //HPBar 매칭
    private void BindHPBar()
    {
        FindGameObject(ref _hPBar, "HP_Bar", true);
        BindImgInitSetting(ref _hPBarImage, "HP_Image", _greyColor, true);
    }

    //SPBar 매칭
    private void BindSPBar()
    {
        BindImgInitSetting(ref _sPBarImage, "SP_Image", _whiteColor, true);
        FindGameObject(ref _sPBar, "SP_Bar", false);
    }

    private void BindMap()
    {
        FindGameObject(ref _minimapMask, "MinimapMask", true);
        FindGameObject(ref _bigmapMask, "BigMapMask", false);
    }

    //탄약 슬롯 초기화, 매칭
    private void BindAmmoSlot()
    {
        for (int i = 0; i < ConstNums.numberOfItemAmmo; i++)
        {
            BindUIBtnInitSetting(ref _ammoBtn[i], $"ammoBtn_{i}", _noneItemBackgroundColor, ItemType.AMMO, i);
            BindTxtInitSetting(ref _ammoTxt[i], $"ammoTxt_{i}", "0", _noneItemImageColor);
            BindImgInitSetting(ref _ammoImg[i], $"ammoImg_{i}", _noneItemImageColor, true);
        }
    }

    //무기 슬롯 초기화, 매칭
    private void BindWeaponSlot()
    {
        for (int i = 0; i < ConstNums.numberOfPlayerSlot; i++)
        {
            ItemType itemType;
            if (i < ConstNums.subWeaponIndex)
            {
                itemType = ItemType.WEAPONGUN;
            }
            else if (i == ConstNums.subWeaponIndex)
            {
                itemType = ItemType.WEAPONSUB;
            }
            else
            {
                itemType = ItemType.WEAPONTHROW;
            }

            BindUIBtnInitSetting(ref _weaponBtn[i], $"weaponBtn_{i}", _clearColor, itemType, i);
            BindTxtInitSetting(ref _weaponTxt[i], $"weaponTxt_{i}", "", _whiteColor);
            BindImgInitSetting(ref _weaponImg[i], $"weaponImg_{i}", _alphaZeroColor, true);
            BindImgInitSetting(ref _weaponCoolImg[i], $"weaponCoolImg_{i}", _alphaZeroColor, true);
            BindImgInitSetting(ref _weaponSelectBox[i], $"weaponBoxImg_{i}", _alphaZeroColor, true);
        }
        BindTxtInitSetting(ref _throwWeaponCount, "weaponTxt_itemCount", "", _whiteColor);
    }

    //현재 총알 슬롯 초기화, 매칭
    private void BindCurAmmoSlot()
    {
        for (int i = 0; i < _curAmmoViewNum; i++)
        {
            BindImgInitSetting(ref _curAmmoImg[i], $"bulletImg_{i}", _alphaOnColor, true);
            BindTxtInitSetting(ref _curAmmoTxt[i], $"bulletTxt_{i}", "0", _whiteColor);
        }
        FindGameObject(ref _curAmmoView, "BulletView", false);
    }

    //방어구 슬롯 초기화, 매칭
    private void BindDefSlot()
    {
        for (int i = 0; i < ConstNums.numberOfItemDefensive; i++)
        {
            BindUIBtnInitSetting(ref _defBtn[i], $"defBtn_{i}", _clearColor, ItemType.DEFENSIVE, i);
            BindTxtInitSetting(ref _defTxt[i], $"defTxt_{i}", "", _whiteColor);
            BindImgInitSetting(ref _defImg[i], $"defImg_{i}", _alphaZeroColor, true);
        }
    }

    //회복류 슬롯 초기화, 매칭
    private void BindRecoverySlot()
    {
        for (int i = 0; i < ConstNums.numberOfItemRecovery; i++)
        {
            BindUIBtnInitSetting(ref _recoverBtn[i], $"recoverBtn_{i}", _noneItemBackgroundColor, ItemType.RECOVERY, i);
            BindTxtInitSetting(ref _recoverTxt[i], $"recoverTxt_{i}", "0", _noneItemImageColor);
            BindImgInitSetting(ref _recoverImg[i], $"recoverImg_{i}", _noneItemImageColor, true);
        }
    }

    //배율 슬롯 초기화
    private void BindScopeSlot()
    {
        FindGameObject(ref _scopeView, "ScopeView", true);
        for (int i = 0; i < ConstNums.numberOfItemScope; i++)
        {
            int lense = 0;
            lense = (i == 0) ? lense = 1 : lense = i << 1;
            BindBtnInitSetting(ref _scopeBtn[i], $"scopeBtn_{i}", _noneItemBackgroundColor);
            BindTxtInitSetting(ref _scopeTxt[i], $"scopeTxt_{i}", $"{lense}x", _whiteColor);

            lense = i;
            // 배율 버튼 이벤트 매칭
            _scopeBtn[lense].onClick.AddListener(() => ChangeEyesight(lense));
            _scopeBtn[i].gameObject.SetActive(false);
        }
        _scopeBtn[0].gameObject.SetActive(true);
        _scopeBtn[0].GetComponent<Image>().color = _alphaOnColor;
    }

    // 각종 알림창
    private void BindNotiWindowView()
    {
        FindGameObject(ref _notiImage, "Notiimage", true);

        BindImgInitSetting(ref _pressFImg, "pressFImg", _alphaOnColor, true);
        BindImgInitSetting(ref _pressFNameImg, "pressFNameImg", _alphaOnColor, true);
        BindTxtInitSetting(ref _pressFNameTxt, "pressFNameTxt", "", _whiteColor);
        FindGameObject(ref _pressFView, "PressFView", false);

        BindTxtInitSetting(ref _actionTxt, "ActionTxt", "", _oragngeColor);
        BindImgInitSetting(ref _actionBackgroundImg, "ActionBackgroundImg", _alphaOnColor, true);
        BindImgInitSetting(ref _actionImg, "ActionImg", _whiteColor, true);
        FindGameObject(ref _actionView, "ActionView", false);

        _informView = GameObject.Find("InformView");
        _informPrefab = Resources.Load<GameObject>("TextResources/InformPrefab");
    }

    private void BindNetworkView()
    {
        _aliveTxt = GameObject.Find("AliveText").GetComponent<TMP_Text>();
        _timeTxt = GameObject.Find("TimeText").GetComponent<TMP_Text>();
        _playerCountView = GameObject.Find("PlayerCountView");
        _timeView = GameObject.Find("TimeView");
        _topLogView = GameObject.Find("TopLogView");
        _botLogView = GameObject.Find("BotLogView");
        _guideView = GameObject.Find("GuideView");
        _topKillPrefab = Resources.Load<GameObject>("TextResources/TopKillLogPrefab");
        _botKillPrefab = Resources.Load<GameObject>("TextResources/BotKillLogPrefab");
        _guidePrefab = Resources.Load<GameObject>("TextResources/GuidePrefab");
    }

    private void BindEndingView()
    {
        _endingView = GameObject.Find("EndingView");
        _endingTxt = GameObject.Find("EndingTxt").GetComponent<TMP_Text>();
        _myRankingTxt = GameObject.Find("MyRankingTxt").GetComponent<TMP_Text>();
        _ranking = GameObject.Find("Ranking").GetComponent<TMP_Text>();
        _rankingTxt = GameObject.Find("RankingTxt").GetComponent<TMP_Text>();
        _killCount = GameObject.Find("KillCount").GetComponent<TMP_Text>();
        _killCountTxt = GameObject.Find("KillCountTxt").GetComponent<TMP_Text>();
        _watchingButton = GameObject.Find("WatchingButton");
        _leaveButton = GameObject.Find("LeaveButton");
        _watchingView = GameObject.Find("WatchingView");
        _endingTxt.gameObject.SetActive(false);
        _myRankingTxt.gameObject.SetActive(false);
        _ranking.gameObject.SetActive(false);
        _rankingTxt.gameObject.SetActive(false);
        _killCount.gameObject.SetActive(false);
        _killCountTxt.gameObject.SetActive(false);
        _watchingButton.gameObject.SetActive(false);
        _leaveButton.gameObject.SetActive(false);
        _watchingView.gameObject.SetActive(false);
        _endingView.SetActive(false);
    }

    private void FindGameObject(ref GameObject gameObject, string objectName, bool active)
    {
        GameObject obj = null;
        obj = GameObject.Find(objectName);
        Debug.Assert(obj != null, $"Can't find {objectName}.");
        gameObject = obj;
        gameObject.gameObject.SetActive(active);
    }

    private void BindUIBtnInitSetting(ref Button btn, string objectName, Color32 color, ItemType itemType, int slotIndex)
    {
        GameObject obj = null;
        obj = GameObject.Find(objectName);
        Debug.Assert(obj != null, $"Can't find {objectName}.");
        btn = obj.GetComponent<Button>();
        btn.GetComponent<Image>().color = color;
        btn.GetComponent<UIButton>().EnterInitialValue(itemType, slotIndex);
    }

    private void BindBtnInitSetting(ref Button btn, string objectName, Color32 color)
    {
        GameObject obj = null;
        obj = GameObject.Find(objectName);
        Debug.Assert(obj != null, $"Can't find {objectName}.");
        btn = obj.GetComponent<Button>();
        btn.GetComponent<Image>().color = color;
    }

    private void BindTxtInitSetting(ref TextMeshProUGUI txt, string objectName, string text, Color32 color)
    {
        GameObject obj = null;
        obj = GameObject.Find(objectName);
        Debug.Assert(obj != null, $"Can't find {objectName}.");
        txt = obj.GetComponent<TextMeshProUGUI>();
        txt.text = text;
        txt.color = color;
        txt.raycastTarget = false;
    }

    private void BindImgInitSetting(ref Image img, string objectName, Color32 color, bool active)
    {
        GameObject obj = null;
        obj = GameObject.Find(objectName);
        Debug.Assert(obj != null, $"Can't find {objectName}.");
        img = obj.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        img.gameObject.SetActive(active);
    }
    #endregion

    #region ClickEvent
    private void ClickEventMatching()
    {
        // 무기 버튼 이벤트 매칭
        for (int i = 0; i < ConstNums.numberOfPlayerSlot; i++)
        {
            int weaponIndex = i;
            _weaponBtn[weaponIndex].onClick.AddListener(() => ChangeSlot(weaponIndex));
        }

        // 회복 버튼 이벤트 매칭
        for (int i = 0; i < ConstNums.numberOfItemRecovery; i++)
        {
            int recoverIndex = i;
            _recoverBtn[i].onClick.AddListener(() => UsingItem(recoverIndex));
        }

        // 배율 버튼 이벤트 매칭은 초기화할 때 함께 해준다.
    }

    // OnClickEvent() ~
    private void ChangeSlot(int slotIndex)
    {
        _player.TryChangeSlot(slotIndex);
    }

    //recoverBtn Click
    private void UsingItem(int slotIndex)
    {
        _player.CheckUsingItem(slotIndex);
    }

    //eyeBtn Click
    private void ChangeEyesight(int slotIndex)
    {
        _playerInfo.ChangeScope(slotIndex * _scopeConvert);
    }

    public void OnWatchingButtonClicked()
    {
        _endingView.SetActive(false);
        PlayerManager.Instance.MoveCameraToOthers();
        _watchingView.SetActive(true);
    }

    public void OnLeaveButtonClicked() =>
        NetworkManager.Instance.ExitGame();

    public void OnNextButtonClicked() =>
        PlayerManager.Instance._otherCamera.GetComponent<CameraManagerInWorld>()
            .FindNext();
        
    public void OnPrevButtonClicked() =>
        PlayerManager.Instance._otherCamera.GetComponent<CameraManagerInWorld>()
            .FindPrev();


    //Item별 Switch문 구성
    //Item마다 작동방식이 달라서 Switch문으로 구성.
    private void CallDropItemByItemType(UIButton uiBtn)
    {
        int slotIndex = uiBtn.GetSlotIndex();
        switch (uiBtn.GetItemType())
        {
            case ItemType.WEAPONGUN:
                {
                    _player.DropWeaponGun(slotIndex);
                }
                break;
            case ItemType.WEAPONSUB:
                {
                    _player.DropWeaponSub(slotIndex);
                }
                break;
            case ItemType.WEAPONTHROW:
                {
                    _player.DropWeaponThrow(slotIndex);
                }
                break;
            case ItemType.RECOVERY:
                {
                    _player.DropItemRecovery(slotIndex);
                }
                break;
            case ItemType.DEFENSIVE:
                break;
            case ItemType.AMMO:
                {
                    _player.DropItemAmmo(slotIndex);
                }
                break;
            default:
                {
                    Debug.Log("uiBtn.ItemType is not valid.");
                }
                break;
        }
    }
    #endregion
    public void PlayerInfoViewOnOff(bool onOff)
    {
        _playerInfoView.SetActive(onOff);
    }

    public void SendPlayerData(Player _player, PlayerInfo _playerInfo)
    {
        UIManager._player = _player;
        UIManager._playerInfo = _playerInfo;

        RefreshSlotWeapon(ConstNums.subWeaponIndex, _playerInfo.GetFist());
    }

    //0721
    public void ChangeCurSlot(int curSlot)
    {
        if (curSlot > ConstNums.weaponThrowIndex)
        {
            curSlot = ConstNums.weaponThrowIndex;
        }
        _weaponBtn[_curSlot].GetComponent<Image>().color = _clearColor;
        _weaponSelectBox[_curSlot].color = _clearColor;
        _weaponBtn[curSlot].GetComponent<Image>().color = _alphaOnColor;
        _weaponSelectBox[curSlot].color = _whiteColor;
        _curSlot = curSlot;

        if (_curSlot < ConstNums.subWeaponIndex)
        {
            _curAmmoView.gameObject.SetActive(true);

            int loadedBullet = _playerInfo.GetLoadedBullet(_curSlot);
            int havingBullet = _playerInfo.GetHavingBullet(_playerInfo.CastIntBulletType(_curSlot));
            RefreshSlotBullet(loadedBullet, havingBullet);
        }
        else
        {
            _curAmmoView.gameObject.SetActive(false);
        }
    }

    // 아이템을 버리는 Direction을 구하는 메서드   
    public Vector2 GetDitchItemDirection()
    {
        return -(_player.CalcAttackDir());
    }

    private void UpdateCursor()
    {
        cursor.transform.position = Input.mousePosition;
    }

    public void ChangeActiveCursor(bool isChange)
    {
        Cursor.visible = !isChange;
        cursor.SetActive(isChange);
    }

    #region RefreshSlot
    //07.18. BellEmpty. 
    /// <summary>
    /// Player의 HP가 변동될 때마다 부르는 함수
    /// </summary>
    public void RefreshHPBar()
    {
        float HPRatio;
        if (_isDead == false)
        {
            HPRatio = _playerInfo.GetCurrentHP() / _playerInfo.GetMaxHP();
            _hPBarImage.fillAmount = HPRatio;
        }
        else
        {
            HPRatio = 0;
            _hPBarImage.fillAmount = HPRatio;
        }

        byte green = 255;
        byte blue = 255;

        if (HPRatio < _redHPRatio)
        {
            green = (byte)(int)(HPRatio * 255);
            blue = (byte)(int)(HPRatio * 255);
        }
        
        if (Mathf.Approximately(HPRatio, _maxHPRatio))
        {
            _hPBarImage.color = _greyColor;
        }
        else
        {
            _hPBarImage.GetComponent<Image>().color = new Color32(255, green, blue, 255);
        }
    }

    /// <summary>
    /// Player의 SP가 변동될 때마다 부르는 함수
    /// </summary>
    public void RefreshSPBar()
    {
        if (_isDead == false)
        {
            _sPBar.SetActive(true);
            _sPBarImage.fillAmount = _playerInfo.GetCurrentSP() / _playerInfo.GetMaxSP();
        }

        if (_sPBarImage.fillAmount <= 0)
        {
            _sPBar.SetActive(false);
        }
    }

    /// <summary>
    /// Player의 Weapon이 변동될 때마다 부르는 함수
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <param name="weapon"></param>
    public void RefreshSlotWeapon(int slotIndex, Weapon weapon)
    {
        if (weapon == null)
        {
            _weaponBtn[slotIndex].GetComponent<Image>().color = _clearColor;
            _weaponTxt[slotIndex].text = "";
            _weaponImg[slotIndex].color = _alphaZeroColor;
        }
        else
        {
            _weaponImg[slotIndex].sprite = weapon.gameObject.GetComponent<SpriteRenderer>().sprite;
            _weaponImg[slotIndex].color = _whiteColor;
            _weaponTxt[slotIndex].text = ($"{weapon.itemData.itemName}");
        }
    }

    public void RefreshSlotWeaponThrow(WeaponThrow weapon)
    {
        int throwIndex = ConstNums.weaponThrowIndex;
        if (weapon == null || weapon.itemData.itemCount == 0)
        {
            _weaponBtn[throwIndex].GetComponent<Image>().color = _clearColor;
            _weaponTxt[throwIndex].text = "";
            _weaponImg[throwIndex].color = _alphaZeroColor;
            _throwWeaponCount.text = "";
        }
        else
        {
            _weaponImg[throwIndex].sprite = weapon.gameObject.GetComponent<SpriteRenderer>().sprite;
            _weaponImg[throwIndex].color = _whiteColor;
            _weaponTxt[throwIndex].text = ($"{weapon.itemData.itemName}");
            _throwWeaponCount.text = ($"{weapon.itemData.itemCount}");
        }
    }

    /// <summary>
    /// Player의 DefensiveItem 정보가 변동될 때마다 부르는 함수
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <param name="def"></param>
    public void RefreshSlotDef(int slotIndex, ItemDefensive def)
    {
        _defBtn[slotIndex].GetComponent<Image>().color = _alphaOnColor;
        _defImg[slotIndex].sprite = def.gameObject.GetComponent<SpriteRenderer>().sprite;
        _defImg[slotIndex].color = _whiteColor;
        _defTxt[slotIndex].color = _whiteColor;
        _defTxt[slotIndex].text = $"Lv {def.itemData.level.ToString()}";
    }

    /// <summary>
    /// Player의 RecoveryItem 개수가 변동될 때마다 부르는 함수
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <param name="rec"></param>
    public void RefreshSlotRecovery(ItemRecovery rec)
    {
        int slotIndex = _playerInfo.CastIntRecoverType(rec);

        _recoverTxt[slotIndex].text = rec.itemData.itemCount.ToString();
        if (rec.itemData.itemCount > 0)
        {
            _recoverBtn[slotIndex].GetComponent<Image>().color = _alphaOnColor;
            _recoverImg[slotIndex].color = _whiteColor;
            _recoverTxt[slotIndex].color = _whiteColor;
        }
        else
        {
            _recoverBtn[slotIndex].GetComponent<Image>().color = _noneItemBackgroundColor;
            _recoverImg[slotIndex].color = _noneItemImageColor;
            _recoverTxt[slotIndex].color = _noneItemImageColor;
        }
    }

    /// <summary>
    /// Player의 HavingBullet 개수가 변동될 때마다 부르는 함수
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <param name="bulletCount"></param>
    public void RefreshSlotAmmo(int slotIndex, int bulletCount)
    {
        _ammoTxt[slotIndex].text = bulletCount.ToString();

        if (bulletCount > 0)
        {
            _ammoBtn[slotIndex].GetComponent<Image>().color = _alphaOnColor;
            _ammoImg[slotIndex].color = _whiteColor;
            _ammoTxt[slotIndex].color = _whiteColor;
        }
        else
        {
            _ammoBtn[slotIndex].GetComponent<Image>().color = _noneItemBackgroundColor;
            _ammoImg[slotIndex].color = _noneItemImageColor;
            _ammoTxt[slotIndex].color = _noneItemImageColor;
        }
    }

    /// <summary>
    /// Player의 LoadedBullet의 정보가 변동될 때마다 부르는 함수
    /// 총을 교체할 때도 불러줘야한다.
    /// </summary>
    /// <param name="loadBullet"></param>
    /// <param name="havingBullet"></param>
    public void RefreshSlotBullet(int loadBullet, int havingBullet)
    {
        string str = loadBullet.ToString();
        _curAmmoTxt[0].text = str;
        str = havingBullet.ToString();
        _curAmmoTxt[1].text = str;
    }

    /// <summary>
    /// Player가 소지하지 않고 있던 Scope Item을 먹었을 때 부르는 함수
    /// </summary>
    /// <param name="lensPower"></param>
    /// <param name="curLensPower"></param>
    public void CreateSlotScope(int lensPower, int curLensPower)
    {
        int lensIndex = lensPower / _scopeConvert;

        _scopeBtn[lensIndex].gameObject.SetActive(true);
        for (int i = 0; i < ConstNums.numberOfItemScope; i++)
        {
            _scopeBtn[i].GetComponent<Image>().color = _alphaOnColor;
            _scopeTxt[i].fontSize = _smallFont;
            _scopeBtn[i].GetComponent<RectTransform>().sizeDelta = _smallScopeBtnSize;
        }
        int curLensIndex = curLensPower / _scopeConvert;
        _scopeBtn[curLensIndex].GetComponent<Image>().color = _alphaOnColor;
        _scopeBtn[curLensIndex].GetComponent<RectTransform>().sizeDelta = _bigScopeBtnSize;
        _scopeTxt[curLensIndex].fontSize = _bigFont;
    }

    /// <summary>
    /// Player의 Scope정보가 변할때 마다 부르는 함수
    /// </summary>
    /// <param name="curEyesight"></param>
    public void RefreshSlotScope(int curEyesight)
    {
        RebuildLayout(_scopeView.GetComponent<RectTransform>());

        int lensIndex = (curEyesight - _playerInfo.GetBasicEyeSight())  / _scopeConvert;
        for (int i = 0; i < ConstNums.numberOfItemScope; i++)
        {
            _scopeBtn[i].GetComponent<Image>().color = _alphaOnColor;
            _scopeTxt[i].fontSize = _smallFont;
            _scopeBtn[i].GetComponent<RectTransform>().sizeDelta = _smallScopeBtnSize;
        }
        _scopeBtn[lensIndex].GetComponent<Image>().color = _alphaOnColor;
        _scopeBtn[lensIndex].GetComponent<RectTransform>().sizeDelta = _bigScopeBtnSize;
        _scopeTxt[lensIndex].fontSize = _bigFont;

        RebuildLayout(_scopeView.GetComponent<RectTransform>());
    }

    /// <summary>
    /// slotIndex의 Scope 버튼이 현재 활성화 상태인지 확인하는 함수 
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <returns></returns>
    public bool IsScopeSlotActive(int slotIndex)
    {
        return _scopeBtn[slotIndex].gameObject.activeInHierarchy;
    }

    /// <summary>
    /// Player가 Item Name의 정보만 전달하면 이름과 함께 Press F 이미지 출력
    /// </summary>
    /// <param name="aliveCount"></param>
    public void ActiveOnPressFView(string itemName)
    {
        _pressFView.gameObject.SetActive(true);
        _pressFNameTxt.text = itemName;
    }

    public void ActiveOffPressFView()
    {
        _pressFView.gameObject.SetActive(false);
    }

    public void PlayActionViewOn(string action, float usingCooltime)
    {
        _actionView.SetActive(true);
        _actionTxt.text = action;
        StartCoroutine("ActionViewFillAmount", usingCooltime);
    }

    public void PlayActionViewOff()
    {
        _actionView.SetActive(false);
        StopCoroutine("ActionViewFillAmount");
    }

    public IEnumerator ActionViewFillAmount(float maxValue)
    {
        float value = 0;
        for (float time = 0; time < maxValue; time += Time.deltaTime)
        {
            value = time / maxValue;
            _actionImg.fillAmount = value;
            yield return null;
        }
    }

    public void PlayWeaponCooltimeView(int slotIndex, float attackCooltime)
    {
        _weaponCoolImg[slotIndex].color = _redHalfAlphaColor;
        StartCoroutine(WeaponViewFillAmount(slotIndex, attackCooltime));
    }

    public IEnumerator WeaponViewFillAmount(int slotIndex, float maxValue)
    {
        float value = 0;
        for (float time = 0; time < maxValue; time += Time.deltaTime)
        {
            value = 1 - time / maxValue;
            _weaponCoolImg[slotIndex].fillAmount = value;
            yield return null;
        }
        _weaponCoolImg[slotIndex].color = _alphaZeroColor;
    }
    // 08.09_LookA ~

    // 현재 플레이어 수 표시
    public void RefreshAlive(byte aliveCount)
    {
        _aliveTxt.text = $"{aliveCount}";
    }

    // 자기장 타이머
    public void RefreshTime(string time)
    {
        _timeTxt.text = $"{time}";
    }

    public void RefreshWinner()
    {
        string txt = MakeColor("이겼닭! 오늘은 찜닭이닭", "yellow");
        _endingTxt.text = txt;
    }

    public void RefreshLoser()
    {
        string txt = MakeColor("오늘은 굶어야지...", "white");
        _endingTxt.text = txt;
    }

    public void RefreshRanking(byte ranking, int maxPlayers)
    {
        string txt = MakeColor($"# {ranking}", "yellow");
        _myRankingTxt.text = txt;

        txt = MakeColor($"#{ranking}", "yellow");
        txt = MakeSize(txt, "80");
        _rankingTxt.text = ($"{txt} / {maxPlayers}");
    }

    public void RefreshKills(int kills)
    {
        _killCountTxt.text = ($"{kills}");
    }

    public void CreateInformText(string txt)
    {
        if(_informView.transform.childCount > 0)
        {
            _informView.transform.GetChild(0).GetComponent<FadeEffect>().Stop();
        }

        GameObject informText = Instantiate(_informPrefab);
        informText.transform.SetParent(_informView.transform);
        informText.transform.localScale = Vector3.one;
        informText.transform.position.Set(0, -300, 0);
        informText.GetComponent<TMP_Text>().text = txt;
    }

    // 페이즈 가이드 텍스트 생성
    public void CreateGuide(int phase, float time)
    {
        RebuildLayout(_topLogView.GetComponent<RectTransform>());

        if (_guideView.transform.childCount > 0)
        {
            _guideView.transform.GetChild(0).GetComponent<FadeEffect>().Stop();
        }

        GameObject guideText = Instantiate(_guidePrefab);
        guideText.transform.SetParent(_guideView.transform);
        guideText.transform.localScale = Vector3.one;

        string txt = "";

        if (phase == 0)    // start wait time
        {
            txt = $"게임 대기 중";
        }
        else if (phase % 2 == 1) // Farming Phase
        {
            txt = MakeColor($"Phase {phase} : 자기장 전진까지 {time}초", "yellow");
        }
        else    // Decrease Phase
        {
            txt = MakeColor($"Phase {phase} : 자기장 접근 중! <br>안쪽으로 이동하세요", "red");
        }

        guideText.GetComponent<TMP_Text>().text = txt;
    }

    // 플레이어 사망 시 상단에 닉네임과 함께 텍스트 생성
    public void CreateTopKillLog(string playerName, object[] enemyInfo)
    {
        RebuildLayout(_topLogView.GetComponent<RectTransform>());

        GameObject killLog = Instantiate(_topKillPrefab);
        killLog.transform.SetParent(_topLogView.transform);
        killLog.transform.localScale = Vector3.one;

        string txt = "";

        if ((bool)enemyInfo[(int)InfoIdx.ISMAGNET] == true)
        {
            txt = $"{MakeBold(playerName)} 님이 자기장 밖에서 사망했습니다";
        }
        else
        {
            txt = $"{MakeBold(playerName)} 님이 {MakeBold((string)enemyInfo[(int)InfoIdx.NAME])} 님에게 당하셨습니다";
        }

        killLog.transform.GetChild(0).GetComponent<TMP_Text>().text = txt;
    }

    // 플레이어 사망 시 하단에 킬 수, 닉네임과 함께 텍스트 생성
    public void CreateBottomKillLog(object[] enemyInfo, int kills, bool isDead)
    {
        //RebuildLayout(_topLogView.GetComponent<RectTransform>());

        if (_botLogView.transform.childCount > 0)
        {
            _botLogView.transform.GetChild(0).GetComponent<FadeEffect>().Stop();
        }

        GameObject killLog = Instantiate(_botKillPrefab);
        killLog.transform.SetParent(_botLogView.transform);
        killLog.transform.localScale = Vector3.one;
        string txt = "";

        if ((bool)enemyInfo[(int)InfoIdx.ISMAGNET] == true)
        {
            txt = $"자기장에 의해 사망했습니다.";
        }
        else if (isDead == false)
        {
            txt = MakeSize(MakeBold(MakeColor($"<br>{kills} 킬", "red")), "40");
            txt = $"당신의 {enemyInfo[(int)InfoIdx.WEAPON]}(으)로 인해 {enemyInfo[(int)InfoIdx.NAME]} 님이 사망했습니다. {txt}";
        }
        else
        {
            txt = $"{enemyInfo[(int)InfoIdx.NAME]} 님의 {enemyInfo[(int)InfoIdx.WEAPON]}(으)로 인해 당신이 사망했습니다.";
        }

        killLog.transform.GetChild(0).GetComponent<TMP_Text>().text = txt;
    }

    public string MakeBold(string txt) => $"<b>{txt}</b>";

    public string MakeColor(string txt, string color) => $"<color={color}>{txt}</color>";

    public string MakeSize(string txt, string size) => $"<size={size}>{txt}</size>";

    public void RebuildLayout(RectTransform obj)
    {
        StartCoroutine(CoRebuildLayout(obj));
    }

    public IEnumerator CoRebuildLayout(RectTransform obj)
    {
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(obj);
    }

    // ~ 08.09_LookA

    #endregion
}