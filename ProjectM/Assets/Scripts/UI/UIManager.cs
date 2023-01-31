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
    //�ӽü�ġ. ���Ŀ� �����ؾ��� ������
    public static UIManager Instance;

    #region UI Object
    public GameObject _playerInfoView = null;

    [Header("���¹�")]
    public GameObject _hPBar = null;
    public GameObject _sPBar = null;
    public Image _hPBarImage = null;
    public Image _sPBarImage = null;

    [Header("ź�� ����")]
    public Button[] _ammoBtn = new Button[ConstNums.numberOfItemAmmo];
    public Image[] _ammoImg = new Image[ConstNums.numberOfItemAmmo];
    public TextMeshProUGUI[] _ammoTxt = new TextMeshProUGUI[ConstNums.numberOfItemAmmo];

    [Header("���� ����")]
    public Button[] _weaponBtn = new Button[ConstNums.numberOfPlayerSlot];
    public Image[] _weaponImg = new Image[ConstNums.numberOfPlayerSlot];
    public Image[] _weaponCoolImg = new Image[ConstNums.numberOfPlayerSlot];
    public Image[] _weaponSelectBox = new Image[ConstNums.numberOfPlayerSlot];
    public TextMeshProUGUI[] _weaponTxt = new TextMeshProUGUI[ConstNums.numberOfPlayerSlot];
    public TextMeshProUGUI _throwWeaponCount = new TextMeshProUGUI();

    [Header("���� ź�� ����")]
    public GameObject _curAmmoView = null;
    public Image[] _curAmmoImg = new Image[_curAmmoViewNum];
    public TextMeshProUGUI[] _curAmmoTxt = new TextMeshProUGUI[_curAmmoViewNum];

    [Header("�� ����")]
    public GameObject _defView = null;
    public Button[] _defBtn = new Button[ConstNums.numberOfItemDefensive];
    public Image[] _defImg = new Image[ConstNums.numberOfItemDefensive];
    public TextMeshProUGUI[] _defTxt = new TextMeshProUGUI[ConstNums.numberOfItemDefensive];

    [Header("ȸ���� ����")]
    public GameObject _recoverView = null;
    public Button[] _recoverBtn = new Button[ConstNums.numberOfItemRecovery];
    public Image[] _recoverImg = new Image[ConstNums.numberOfItemRecovery];
    public TextMeshProUGUI[] _recoverTxt = new TextMeshProUGUI[ConstNums.numberOfItemRecovery];

    [Header("���� ��ư")]
    public GameObject _scopeView = null;
    public Button[] _scopeBtn = new Button[ConstNums.numberOfItemScope];
    public TextMeshProUGUI[] _scopeTxt = new TextMeshProUGUI[ConstNums.numberOfItemScope];

    [Header("�̴ϸ� ����")]
    public GameObject _minimapMask = null;
    public GameObject _bigmapMask = null;

    [Header("����")]
    public GameObject _notiImage = null;

    [Header("��Ʈ��ũ �ؽ�Ʈ")]
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

    [Header("�˸�â")]
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

    [Header("����")]
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
        //��Ŭ�� Item Drop ���
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
                //��Ŭ�� ��� ��ư ���� ����
                CallDropItemByItemType(uiBtn);
            }
            else
            {
                Debug.Log("It's not a Button");
            }
        }
    }

    #region BindUI
    //HPBar ��Ī
    private void BindHPBar()
    {
        FindGameObject(ref _hPBar, "HP_Bar", true);
        BindImgInitSetting(ref _hPBarImage, "HP_Image", _greyColor, true);
    }

    //SPBar ��Ī
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

    //ź�� ���� �ʱ�ȭ, ��Ī
    private void BindAmmoSlot()
    {
        for (int i = 0; i < ConstNums.numberOfItemAmmo; i++)
        {
            BindUIBtnInitSetting(ref _ammoBtn[i], $"ammoBtn_{i}", _noneItemBackgroundColor, ItemType.AMMO, i);
            BindTxtInitSetting(ref _ammoTxt[i], $"ammoTxt_{i}", "0", _noneItemImageColor);
            BindImgInitSetting(ref _ammoImg[i], $"ammoImg_{i}", _noneItemImageColor, true);
        }
    }

    //���� ���� �ʱ�ȭ, ��Ī
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

    //���� �Ѿ� ���� �ʱ�ȭ, ��Ī
    private void BindCurAmmoSlot()
    {
        for (int i = 0; i < _curAmmoViewNum; i++)
        {
            BindImgInitSetting(ref _curAmmoImg[i], $"bulletImg_{i}", _alphaOnColor, true);
            BindTxtInitSetting(ref _curAmmoTxt[i], $"bulletTxt_{i}", "0", _whiteColor);
        }
        FindGameObject(ref _curAmmoView, "BulletView", false);
    }

    //�� ���� �ʱ�ȭ, ��Ī
    private void BindDefSlot()
    {
        for (int i = 0; i < ConstNums.numberOfItemDefensive; i++)
        {
            BindUIBtnInitSetting(ref _defBtn[i], $"defBtn_{i}", _clearColor, ItemType.DEFENSIVE, i);
            BindTxtInitSetting(ref _defTxt[i], $"defTxt_{i}", "", _whiteColor);
            BindImgInitSetting(ref _defImg[i], $"defImg_{i}", _alphaZeroColor, true);
        }
    }

    //ȸ���� ���� �ʱ�ȭ, ��Ī
    private void BindRecoverySlot()
    {
        for (int i = 0; i < ConstNums.numberOfItemRecovery; i++)
        {
            BindUIBtnInitSetting(ref _recoverBtn[i], $"recoverBtn_{i}", _noneItemBackgroundColor, ItemType.RECOVERY, i);
            BindTxtInitSetting(ref _recoverTxt[i], $"recoverTxt_{i}", "0", _noneItemImageColor);
            BindImgInitSetting(ref _recoverImg[i], $"recoverImg_{i}", _noneItemImageColor, true);
        }
    }

    //���� ���� �ʱ�ȭ
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
            // ���� ��ư �̺�Ʈ ��Ī
            _scopeBtn[lense].onClick.AddListener(() => ChangeEyesight(lense));
            _scopeBtn[i].gameObject.SetActive(false);
        }
        _scopeBtn[0].gameObject.SetActive(true);
        _scopeBtn[0].GetComponent<Image>().color = _alphaOnColor;
    }

    // ���� �˸�â
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
        // ���� ��ư �̺�Ʈ ��Ī
        for (int i = 0; i < ConstNums.numberOfPlayerSlot; i++)
        {
            int weaponIndex = i;
            _weaponBtn[weaponIndex].onClick.AddListener(() => ChangeSlot(weaponIndex));
        }

        // ȸ�� ��ư �̺�Ʈ ��Ī
        for (int i = 0; i < ConstNums.numberOfItemRecovery; i++)
        {
            int recoverIndex = i;
            _recoverBtn[i].onClick.AddListener(() => UsingItem(recoverIndex));
        }

        // ���� ��ư �̺�Ʈ ��Ī�� �ʱ�ȭ�� �� �Բ� ���ش�.
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


    //Item�� Switch�� ����
    //Item���� �۵������ �޶� Switch������ ����.
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

    // �������� ������ Direction�� ���ϴ� �޼���   
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
    /// Player�� HP�� ������ ������ �θ��� �Լ�
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
    /// Player�� SP�� ������ ������ �θ��� �Լ�
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
    /// Player�� Weapon�� ������ ������ �θ��� �Լ�
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
    /// Player�� DefensiveItem ������ ������ ������ �θ��� �Լ�
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
    /// Player�� RecoveryItem ������ ������ ������ �θ��� �Լ�
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
    /// Player�� HavingBullet ������ ������ ������ �θ��� �Լ�
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
    /// Player�� LoadedBullet�� ������ ������ ������ �θ��� �Լ�
    /// ���� ��ü�� ���� �ҷ�����Ѵ�.
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
    /// Player�� �������� �ʰ� �ִ� Scope Item�� �Ծ��� �� �θ��� �Լ�
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
    /// Player�� Scope������ ���Ҷ� ���� �θ��� �Լ�
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
    /// slotIndex�� Scope ��ư�� ���� Ȱ��ȭ �������� Ȯ���ϴ� �Լ� 
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <returns></returns>
    public bool IsScopeSlotActive(int slotIndex)
    {
        return _scopeBtn[slotIndex].gameObject.activeInHierarchy;
    }

    /// <summary>
    /// Player�� Item Name�� ������ �����ϸ� �̸��� �Բ� Press F �̹��� ���
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

    // ���� �÷��̾� �� ǥ��
    public void RefreshAlive(byte aliveCount)
    {
        _aliveTxt.text = $"{aliveCount}";
    }

    // �ڱ��� Ÿ�̸�
    public void RefreshTime(string time)
    {
        _timeTxt.text = $"{time}";
    }

    public void RefreshWinner()
    {
        string txt = MakeColor("�̰��! ������ ����̴�", "yellow");
        _endingTxt.text = txt;
    }

    public void RefreshLoser()
    {
        string txt = MakeColor("������ �������...", "white");
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

    // ������ ���̵� �ؽ�Ʈ ����
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
            txt = $"���� ��� ��";
        }
        else if (phase % 2 == 1) // Farming Phase
        {
            txt = MakeColor($"Phase {phase} : �ڱ��� �������� {time}��", "yellow");
        }
        else    // Decrease Phase
        {
            txt = MakeColor($"Phase {phase} : �ڱ��� ���� ��! <br>�������� �̵��ϼ���", "red");
        }

        guideText.GetComponent<TMP_Text>().text = txt;
    }

    // �÷��̾� ��� �� ��ܿ� �г��Ӱ� �Բ� �ؽ�Ʈ ����
    public void CreateTopKillLog(string playerName, object[] enemyInfo)
    {
        RebuildLayout(_topLogView.GetComponent<RectTransform>());

        GameObject killLog = Instantiate(_topKillPrefab);
        killLog.transform.SetParent(_topLogView.transform);
        killLog.transform.localScale = Vector3.one;

        string txt = "";

        if ((bool)enemyInfo[(int)InfoIdx.ISMAGNET] == true)
        {
            txt = $"{MakeBold(playerName)} ���� �ڱ��� �ۿ��� ����߽��ϴ�";
        }
        else
        {
            txt = $"{MakeBold(playerName)} ���� {MakeBold((string)enemyInfo[(int)InfoIdx.NAME])} �Կ��� ���ϼ̽��ϴ�";
        }

        killLog.transform.GetChild(0).GetComponent<TMP_Text>().text = txt;
    }

    // �÷��̾� ��� �� �ϴܿ� ų ��, �г��Ӱ� �Բ� �ؽ�Ʈ ����
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
            txt = $"�ڱ��忡 ���� ����߽��ϴ�.";
        }
        else if (isDead == false)
        {
            txt = MakeSize(MakeBold(MakeColor($"<br>{kills} ų", "red")), "40");
            txt = $"����� {enemyInfo[(int)InfoIdx.WEAPON]}(��)�� ���� {enemyInfo[(int)InfoIdx.NAME]} ���� ����߽��ϴ�. {txt}";
        }
        else
        {
            txt = $"{enemyInfo[(int)InfoIdx.NAME]} ���� {enemyInfo[(int)InfoIdx.WEAPON]}(��)�� ���� ����� ����߽��ϴ�.";
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