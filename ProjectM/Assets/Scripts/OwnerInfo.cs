public struct OwnerInfo
{
    public string _name;
    public string _weapon;
    public bool _isMagnet;

    // 자기장(Magnetic) 생성자
    public OwnerInfo(bool isMagnet)
    {
        this._name = "";
        this._weapon = "";
        this._isMagnet = isMagnet;
    }

    // 총알(Bullet) 생성자
    public OwnerInfo(string name, string weapon)
    {
        this._name = name;
        this._weapon = weapon;
        this._isMagnet = false;
    }

    public object[] GetObjects() => new object[] { _name, _weapon, _isMagnet };
}
