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
    public static UIManager _Instance;

    #region UI Object
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
    public Image _reloadingImg = null;
    public Image _recoveringImg = null;

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
    private Color32 _whiteColor = Color.white;
    private Color32 _clearColor = Color.clear;
    #endregion

    private void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
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
        BindNotiImage();
        BindEndingView();

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
            pointerEventData.position = Input.mousePosition;
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            graphicRaycaster.Raycast(pointerEventData, raycastResults);

            if (raycastResults.Count > 0)
            {
                UIButton uiBtn = null;
                Debug.Log(raycastResults[0]);
                if (raycastResults[0].gameObject.TryGetComponent<UIButton>(out uiBtn))
                {
                    Debug.Log("It's a Button");

                    //��Ŭ�� ��� ��ư ���� ����
                    ValidCheckDitchItem(uiBtn);
                }
                else
                {
                    Debug.Log("It's not a Button");
                }
            }
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
            if(notiOn == true)
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

    #region BindUI
    //HPBar ��Ī
    private void BindHPBar()
    {
        GameObject obj = null;
        Image img = null;

        obj = GameObject.Find("HP_Bar");
        if (obj == null)
        {
            Debug.Log("Can't find HP_Bar");
        }
        else
        {
            _hPBar = obj;
        }

        obj = GameObject.Find("HP_Image");
        if (obj.TryGetComponent<Image>(out img))
        {
            _hPBarImage = img;
        }
        else
        {
            Debug.Log("Can't find HP_Image");
        }
    }

    //SPBar ��Ī
    private void BindSPBar()
    {
        GameObject obj = null;
        Image img = null;

        obj = GameObject.Find("SP_Bar");
        if (obj == null)
        {
            Debug.Log("Can't find SP_Bar");
        }
        else
        {
            _sPBar = obj;
        }

        obj = GameObject.Find("SP_Image");
        if (obj.TryGetComponent<Image>(out img))
        {
            _sPBarImage = img;
        }
        else
        {
            Debug.Log("Can't find SP_Image");
        }
        _sPBar.SetActive(false);
    }

    private void BindMap()
    {
        GameObject obj1 = null;
        GameObject obj2 = null;

        obj1 = GameObject.Find("MinimapMask");
        if (obj1 == null)
        {
            Debug.Log("Can't find minimap");
        }
        else
        {
            _minimapMask = obj1;
        }
        obj2 = GameObject.Find("BigMapMask");
        if (obj2 == null)
        {
            Debug.Log("Can't find bigmap");
        }
        else
        {
            _bigmapMask = obj2;
            _bigmapMask.SetActive(false);
        }

    }

    //ź�� ���� �ʱ�ȭ, ��Ī
    private void BindAmmoSlot()
    {
        Button btn = null;
        TextMeshProUGUI txt = null;
        Image img = null;
        GameObject obj = null;

        for (int i = 0; i < ConstNums.numberOfItemAmmo; i++)
        {
            obj = GameObject.Find($"ammoBtn_{i}");
            if (obj.TryGetComponent<Button>(out btn))
            {
                _ammoBtn[i] = btn;
                _ammoBtn[i].GetComponent<Image>().color = _noneItemBackgroundColor;
                _ammoBtn[i].GetComponent<UIButton>().EnterInitialValue(ItemType.AMMO, i);
            }
            else
            {
                Debug.Log($"Can't Find ammoBtn_{i}");
            }

            obj = GameObject.Find($"ammoTxt_{i}");
            if (obj.TryGetComponent<TextMeshProUGUI>(out txt))
            {
                _ammoTxt[i] = txt;
                _ammoTxt[i].color = _noneItemImageColor;
                _ammoTxt[i].text = "0";
                _ammoTxt[i].raycastTarget = false;
            }
            else
            {
                Debug.Log($"Can't Find ammoTxt_{i}");
            }

            obj = GameObject.Find($"ammoImg_{i}");
            if (obj.TryGetComponent<Image>(out img))
            {
                _ammoImg[i] = img;
                _ammoImg[i].color = _noneItemImageColor;
                _ammoImg[i].raycastTarget = false;
            }
            else
            {
                Debug.Log($"Can't Find ammoImg_{i}");
            }
        }
    }

    //���� ���� �ʱ�ȭ, ��Ī
    private void BindWeaponSlot()
    {
        Button btn = null;
        TextMeshProUGUI txt = null;
        Image img = null;
        GameObject obj = null;

        obj = GameObject.Find("weaponTxt_itemCount");
        if(obj.TryGetComponent<TextMeshProUGUI>(out txt))
        {
            _throwWeaponCount = txt;
            _throwWeaponCount.text = "";
            _throwWeaponCount.raycastTarget = false;
        }

        for (int i = 0; i < ConstNums.numberOfPlayerSlot; i++)
        {
            obj = GameObject.Find($"weaponBtn_{i}");
            if (obj.TryGetComponent<Button>(out btn))
            {
                _weaponBtn[i] = btn;
                _weaponBtn[i].GetComponent<Image>().color = _clearColor;
                if (i < ConstNums.subWeaponIndex)
                {
                    _weaponBtn[i].GetComponent<UIButton>().EnterInitialValue(ItemType.WEAPONGUN, i);
                }
                else if (i == ConstNums.subWeaponIndex)
                {
                    _weaponBtn[i].GetComponent<UIButton>().EnterInitialValue(ItemType.WEAPONSUB, i);
                }
                else
                {
                    _weaponBtn[i].GetComponent<UIButton>().EnterInitialValue(ItemType.WEAPONTHROW, i);
                }
            }
            else
            {
                Debug.Log($"Can't Find weaponBtn_{i}");
            }

            obj = GameObject.Find($"weaponTxt_{i}");
            if (obj.TryGetComponent<TextMeshProUGUI>(out txt))
            {
                _weaponTxt[i] = txt;
                _weaponTxt[i].text = "";
                _weaponTxt[i].raycastTarget = false;
            }
            else
            {
                Debug.Log($"Can't Find weaponTxt_{i}");
            }

            obj = GameObject.Find($"weaponImg_{i}");
            if (obj.TryGetComponent<Image>(out img))
            {
                _weaponImg[i] = img;
                _weaponImg[i].color = _alphaZeroColor;
                _weaponImg[i].raycastTarget = false;
            }
            else
            {
                Debug.Log($"Can't Find weaponImg_{i}");
            }

            obj = GameObject.Find($"weaponBoxImg_{i}");
            if(obj.TryGetComponent<Image>(out img))
            {
                _weaponSelectBox[i] = img;
                _weaponSelectBox[i].color = _alphaZeroColor;
                _weaponSelectBox[i].raycastTarget = false;
            }
        }
    }

    //���� �Ѿ� ���� �ʱ�ȭ, ��Ī
    private void BindCurAmmoSlot()
    {
        TextMeshProUGUI txt = null;
        Image img = null;
        GameObject obj = null;

        obj = GameObject.Find("BulletView");
        if (obj == null)
        {
            Debug.Log("Can't find bulletView");
        }
        else
        {
            _curAmmoView = obj;
        }

        for (int i = 0; i < _curAmmoViewNum; i++)
        {
            obj = GameObject.Find($"bulletImg_{i}");
            if (obj.TryGetComponent<Image>(out img))
            {
                _curAmmoImg[i] = img;
            }
            else
            {
                Debug.Log($"Can't Find bulletImg_{i}");
            }

            obj = GameObject.Find($"bulletTxt_{i}");
            if (obj.TryGetComponent<TextMeshProUGUI>(out txt))
            {
                _curAmmoTxt[i] = txt;
            }
            else
            {
                Debug.Log($"Can't Find bulletTxt_{i}");
            }
        }
        _curAmmoView.gameObject.SetActive(false);
    }

    //�� ���� �ʱ�ȭ, ��Ī
    private void BindDefSlot()
    {
        Button btn = null;
        TextMeshProUGUI txt = null;
        Image img = null;
        GameObject obj = null;

        for (int i = 0; i < ConstNums.numberOfItemDefensive; i++)
        {
            obj = GameObject.Find($"defBtn_{i}");
            if (obj.TryGetComponent<Button>(out btn))
            {
                _defBtn[i] = btn;
                _defBtn[i].GetComponent<Image>().color = _clearColor;
                _defBtn[i].GetComponent<UIButton>().EnterInitialValue(ItemType.DEFENSIVE, i);
            }
            else
            {
                Debug.Log($"Can't Find defBtn_{i}");
            }

            obj = GameObject.Find($"defTxt_{i}");
            if (obj.TryGetComponent<TextMeshProUGUI>(out txt))
            {
                _defTxt[i] = txt;
                _defTxt[i].color = _alphaZeroColor;
                _defTxt[i].text = "0";
                _defTxt[i].raycastTarget = false;
            }
            else
            {
                Debug.Log($"Can't Find defTxt_{i}");
            }

            obj = GameObject.Find($"defImg_{i}");
            if (obj.TryGetComponent<Image>(out img))
            {
                _defImg[i] = img;
                _defImg[i].color = _alphaZeroColor;
                _defImg[i].raycastTarget = false;
            }
            else
            {
                Debug.Log($"Can't Find defImg_{i}");
            }
        }
    }

    //ȸ���� ���� �ʱ�ȭ, ��Ī
    private void BindRecoverySlot()
    {
        Button btn = null;
        TextMeshProUGUI txt = null;
        Image img = null;
        GameObject obj = null;

        for (int i = 0; i < ConstNums.numberOfItemRecovery; i++)
        {
            obj = GameObject.Find($"recoverBtn_{i}");
            if (obj.TryGetComponent<Button>(out btn))
            {
                _recoverBtn[i] = btn;
                _recoverBtn[i].GetComponent<Image>().color = _noneItemBackgroundColor;
                _recoverBtn[i].GetComponent<UIButton>().EnterInitialValue(ItemType.RECOVERY, i);
            }
            else
            {
                Debug.Log($"Can't Find recoverBtn_{i}");
            }

            obj = GameObject.Find($"recoverTxt_{i}");
            if (obj.TryGetComponent<TextMeshProUGUI>(out txt))
            {
                _recoverTxt[i] = txt;
                _recoverTxt[i].color = _noneItemImageColor;
                _recoverTxt[i].text = "0";
                _recoverTxt[i].raycastTarget = false;
            }
            else
            {
                Debug.Log($"Can't Find recoverTxt_{i}");
            }

            obj = GameObject.Find($"recoverImg_{i}");
            if (obj.TryGetComponent<Image>(out img))
            {
                _recoverImg[i] = img;
                _recoverImg[i].color = _noneItemImageColor;
                _recoverImg[i].raycastTarget = false;
            }
            else
            {
                Debug.Log($"Can't Find recoverImg_{i}");
            }
        }
    }

    //���� ���� �ʱ�ȭ
    private void BindScopeSlot()
    {
        Button btn = null;
        TextMeshProUGUI txt = null;
        GameObject obj = null;

        obj = GameObject.Find("ScopeView");
        _scopeView = obj;

        for (int i = 0; i < ConstNums.numberOfItemScope; i++)
        {
            obj = GameObject.Find($"scopeTxt_{i}");
            if (obj.TryGetComponent<TextMeshProUGUI>(out txt))
            {
                _scopeTxt[i] = txt;
                _scopeTxt[i].color = _whiteColor;
                _scopeTxt[i].raycastTarget = false;
            }
            else
            {
                Debug.Log($"Can't Find scopeTxt_{i}");
            }

            obj = GameObject.Find($"scopeBtn_{i}");
            if (obj.TryGetComponent<Button>(out btn))
            {
                _scopeBtn[i] = btn;
                _scopeBtn[i].GetComponent<Image>().color = _noneItemBackgroundColor;
                int tempI = i;
                // ���� ��ư �̺�Ʈ ��Ī
                _scopeBtn[tempI].onClick.AddListener(() => ChangeEyesight(tempI));
                _scopeBtn[i].gameObject.SetActive(false);
            }
            else
            {
                Debug.Log($"Can't Find scopeBtn_{i}");
            }
        }
        _scopeBtn[0].gameObject.SetActive(true);
        _scopeBtn[0].GetComponent<Image>().color = _alphaOnColor;
    }

    // ���� �˸�â
    private void BindNotiWindowView()
    {
        Image img = null;
        TextMeshProUGUI txt = null;
        GameObject obj = null;

        obj = GameObject.Find("PressFView");
        if (obj == null)
        {
            Debug.Log("Can't Find PressFView");
        }
        else
        {
            _pressFView = obj;
        }

        obj = GameObject.Find("pressFImg");
        if (obj.TryGetComponent<Image>(out img))
        {
            _pressFImg = img;
            _pressFImg.color = _alphaOnColor;
        }
        else
        {
            Debug.Log($"Can't Find pressFImg");
        }

        obj = GameObject.Find("pressFNameImg");
        if (obj.TryGetComponent<Image>(out img))
        {
            _pressFNameImg = img;
            _pressFNameImg.color = _alphaOnColor;
        }
        else
        {
            Debug.Log($"Can't Find pressFNameImg");
        }

        obj = GameObject.Find("pressFNameTxt");
        if (obj.TryGetComponent<TextMeshProUGUI>(out txt))
        {
            _pressFNameTxt = txt;
            _pressFNameTxt.text = "";
        }
        else
        {
            Debug.Log("Can't Find pressFNameTxt");
        }

        _pressFView.gameObject.SetActive(false);

        obj = GameObject.Find("reloadingImg");
        if (obj.TryGetComponent<Image>(out img))
        {
            _reloadingImg = img;
            _reloadingImg.gameObject.SetActive(false);
        }

        obj = GameObject.Find("recoveringImg");
        if (obj.TryGetComponent<Image>(out img))
        {
            _recoveringImg = img;
            _recoveringImg.gameObject.SetActive(false);
        }
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

    private void BindNotiImage()
    {
        _notiImage = GameObject.Find("Notiimage");
        if(_notiImage == null)
        {
            Debug.Log("Can't Find Noti");
        }
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
        _endingTxt.gameObject.SetActive(false);
        _myRankingTxt.gameObject.SetActive(false);
        _ranking.gameObject.SetActive(false);
        _rankingTxt.gameObject.SetActive(false);
        _killCount.gameObject.SetActive(false);
        _killCountTxt.gameObject.SetActive(false);
        _watchingButton.gameObject.SetActive(false);
        _leaveButton.gameObject.SetActive(false);
        _endingView.SetActive(false);
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

    public void OnWatchingButtonClicked() => _endingView.SetActive(false);

    public void OnLeaveButtonClicked() => NetworkManager.ExitGame();

    //Item�� Switch�� ����
    //Item���� �۵������ �޶� Switch������ ����.
    private void ValidCheckDitchItem(UIButton uiBtn)
    {
        int slotIndex = uiBtn.GetSlotIndex();
        switch (uiBtn.GetItemType())
        {
            case ItemType.WEAPONGUN:
                {
                    WeaponGun gun = _playerInfo._slotWeapon[slotIndex].GetWeapon().GetComponent<WeaponGun>();
                    if (gun != null)
                    {
                        int gunBulletType = (int)gun.bulletType;

                        _playerInfo._bulletSlot[gunBulletType].itemCount += _playerInfo._loadedBullet[slotIndex];
                        _playerInfo._loadedBullet[slotIndex] = 0;

                        ItemManager.DitchItem(gun, _playerInfo.transform.position, -(_player.CalcAttackDir()));
                        _playerInfo._slotWeapon[slotIndex].ClearWeapon();

                        RefreshSlotAmmo(gunBulletType, _playerInfo._bulletSlot[gunBulletType].itemCount);
                        RefreshSlotWeapon(slotIndex, _playerInfo._slotWeapon[slotIndex].GetWeapon());
                        
                        // ���� ���� == ���� ���� �� �� TryChangeSlot�ؾ��Ѵ�.
                        if(slotIndex == _curSlot)
                        {
                            _player.TryChangeSlot(ConstNums.subWeaponIndex);
                        }
                    }
                }
                break;
            case ItemType.WEAPONSUB:
                {
                    WeaponSub sub = _playerInfo._slotWeapon[slotIndex].GetWeapon().GetComponent<WeaponSub>();
                    if (sub != _playerInfo._fist)
                    {
                        ItemManager.DitchItem(sub, _playerInfo.transform.position, -(_player.CalcAttackDir()));
                        _playerInfo._slotWeapon[slotIndex].SetWeapon(_playerInfo._fist);
                        RefreshSlotWeapon(slotIndex, _playerInfo._fist);
                        if (_playerInfo.IsSubSlot(_curSlot))
                        {
                            _player.ChangeAnimFist();
                        }
                    }
                }
                break;
            case ItemType.WEAPONTHROW:
                {
                    Debug.Log("Ditch Weapon Throw");
                }
                break;
            case ItemType.RECOVERY:
                {
                    if (_playerInfo._recoverSlot[slotIndex] != null && _playerInfo._recoverSlot[slotIndex].itemCount > 0)
                    {
                        ItemManager.DitchItem(_playerInfo._recoverSlot[slotIndex], _playerInfo.transform.position, -(_player.CalcAttackDir()));
                        RefreshSlotRecovery(_playerInfo._recoverSlot[slotIndex]);
                    }
                }
                break;
            case ItemType.DEFENSIVE:
                break;
            case ItemType.AMMO:
                {
                    if (_playerInfo._bulletSlot[slotIndex] != null && _playerInfo._bulletSlot[slotIndex].itemCount > 0)
                    {
                        ItemManager.DitchItem(_playerInfo._bulletSlot[slotIndex], _playerInfo.transform.position, -(_player.CalcAttackDir()));
                        RefreshSlotAmmo(slotIndex, _playerInfo._bulletSlot[slotIndex].itemCount);
                    }
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

    public void SendPlayerData(Player _player, PlayerInfo _playerInfo)
    {
        UIManager._player = _player;
        UIManager._playerInfo = _playerInfo;

        RefreshSlotWeapon(ConstNums.subWeaponIndex, UIManager._playerInfo._fist);
    }

    //0721
    public void ChangeCurSlot(int curSlot)
    {
        if(curSlot > ConstNums.throwWeaponIndex)
        {
            curSlot = ConstNums.throwWeaponIndex;
        }
        _weaponBtn[_curSlot].GetComponent<Image>().color = _clearColor;
        _weaponSelectBox[_curSlot].color = _clearColor;
        _weaponBtn[curSlot].GetComponent<Image>().color = _alphaOnColor;
        _weaponSelectBox[curSlot].color = _whiteColor;
        _curSlot = curSlot;

        if (_curSlot < ConstNums.subWeaponIndex)
        {
            _curAmmoView.gameObject.SetActive(true);
            RefreshSlotBullet(_playerInfo._loadedBullet[_curSlot], _playerInfo.GetHavingBullet(_playerInfo.CastIntBulletType(_curSlot)));
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
            HPRatio = _playerInfo._currentHP / _playerInfo._maxHP;
            _hPBarImage.fillAmount = HPRatio;
        }
        else
        {
            HPRatio = 0;
            _hPBarImage.fillAmount = HPRatio;
        }
        byte red = 255;
        byte green = 255;
        byte blue = 255;
        byte alpha = 255;

        if (HPRatio == _maxHPRatio)
        {
            alpha = 100;
        }

        if (HPRatio < _redHPRatio)
        {
            green = (byte)(int)(HPRatio * 255);
            blue = (byte)(int)(HPRatio * 255);
        }

        _hPBarImage.GetComponent<Image>().color = new Color32(red, green, blue, alpha);
    }

    /// <summary>
    /// Player�� SP�� ������ ������ �θ��� �Լ�
    /// </summary>
    public void RefreshSPBar()
    {
        if (_isDead == false)
        {
            _sPBar.SetActive(true);
            _sPBarImage.fillAmount = _playerInfo._currentSP / _playerInfo._maxSP;
        }
        else
        {
            _sPBarImage.fillAmount = 0;
            _sPBar.SetActive(false);
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
            _weaponTxt[slotIndex].text = ($"{weapon.itemName}");
        }
    }

    public void RefreshSlotThrowWeapon(Weapon weapon)
    {
        int throwIndex = ConstNums.throwWeaponIndex;
        if (weapon == null)
        {
            _weaponBtn[throwIndex].GetComponent<Image>().color = _clearColor;
            _weaponTxt[throwIndex].text = "";
            _weaponImg[throwIndex].color = _alphaZeroColor;
            _throwWeaponCount.color = _alphaZeroColor;
        }
        else
        {
            _weaponImg[throwIndex].sprite = weapon.gameObject.GetComponent<SpriteRenderer>().sprite;
            _weaponImg[throwIndex].color = _whiteColor;
            _weaponTxt[throwIndex].text = ($"{weapon.itemName}");
            _throwWeaponCount.text = ($"{weapon.itemCount}");
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
        _defTxt[slotIndex].text = $"Lv {def.level.ToString()}";
    }

    /// <summary>
    /// Player�� RecoveryItem ������ ������ ������ �θ��� �Լ�
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <param name="rec"></param>
    public void RefreshSlotRecovery(ItemRecovery rec)
    {
        int slotIndex = _playerInfo.CastIntRecoverType(rec);

        _recoverTxt[slotIndex].text = rec.itemCount.ToString();
        if (rec.itemCount > 0)
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

        int lensIndex = (curEyesight - _playerInfo._basicEyesight) / _scopeConvert;
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

    public void SetActiveReloadingView(bool onOff)
    {
        _reloadingImg.gameObject.SetActive(onOff);
    }

    public void SetActiveRecoveringView(bool onOff)
    {
        _recoveringImg.gameObject.SetActive(onOff);
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
            txt = MakeColor($"Phase {phase} : �ڱ��� ���� ��! �������� �̵��ϼ���", "red");
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