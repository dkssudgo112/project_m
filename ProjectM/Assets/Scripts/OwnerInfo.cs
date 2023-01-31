public struct OwnerInfo
{
    public string _name;
    public string _weapon;
    public bool _isMagnet;
    public int _id;

    // 자기장(Magnetic) 생성자
    public OwnerInfo(bool isMagnet)
    {
        this._name = "";
        this._weapon = "";
        this._isMagnet = isMagnet;
        this._id = 0;
    }

    // 총알(Bullet) 생성자
    public OwnerInfo(string name, string weapon, int id)
    {
        this._name = name;
        this._weapon = weapon;
        this._isMagnet = false;
        this._id = id;
    }

    public object[] GetObjects() => new object[] { _name, _weapon, _isMagnet, _id };
}
