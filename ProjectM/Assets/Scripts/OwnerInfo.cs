public struct OwnerInfo
{
    public string _name;
    public string _weapon;
    public bool _isMagnet;

    // �ڱ���(Magnetic) ������
    public OwnerInfo(bool isMagnet)
    {
        this._name = "";
        this._weapon = "";
        this._isMagnet = isMagnet;
    }

    // �Ѿ�(Bullet) ������
    public OwnerInfo(string name, string weapon)
    {
        this._name = name;
        this._weapon = weapon;
        this._isMagnet = false;
    }

    public object[] GetObjects() => new object[] { _name, _weapon, _isMagnet };
}
