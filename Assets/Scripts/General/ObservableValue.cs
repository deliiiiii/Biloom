using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

[Serializable]
public class ObservableValue<T,TCLASS>
{
    [SerializeField]
    private T value;
    //private readonly string valueType;
    private readonly TCLASS valueClass;
    //delegate void OnValueChangeDelegate(T oldValue, T newValue, string valueType);
    delegate void OnValueChangeDelegate(T oldValue, T newValue, TCLASS valueClass);
    event OnValueChangeDelegate OnValueChangeEvent;
    //public ObservableValue(T value, string valueType)
    public ObservableValue(T value, TCLASS valueClass)
    {
        this.value = value;
        this.valueClass = valueClass;
        this.OnValueChangeEvent += OnValueChange;

    }
    public T Value
    {
        get => value;
        set
        {
            T oldValue = this.value;
            if (this.value.Equals(value))
                return;
            //if (typeof(T) == typeof(int) && (int.Parse(value.ToString()) < 0))
            //    return;

            //if (valueType == 7 && (int.Parse(value.ToString()) < 0))
            //{
            //    T t = (T)(object)Convert.ToInt32(0);
            //    this.value = t;
            //}

            this.value = value;
            OnValueChangeEvent?.Invoke(oldValue, value, this.valueClass);
        }
    }
    //public void OnValueChange(T oldValue, T newValue, string valueType)
    public void OnValueChange(T oldValue, T newValue, TCLASS valueClass)
    {

        //if (valueType == 0 && typeof(T) == typeof(int) && int.Parse(newValue.ToString()) >= 2)
        //{
        //    //Debug.Log("int�ﵽ��ֵ2 ����");
        //}
        //if (valueType == 1 && typeof(T) == typeof(float) && float.Parse(newValue.ToString()) >= 0.2f)
        //{
        //    //Debug.Log("float�ﵽ��ֵ0.2f ����");
        //}


        if(valueClass is Momentus)
            ((Momentus)((object)valueClass)).OnIsInMakerChange();
        if (valueClass is SwipeMenu)
            ((SwipeMenu)((object)valueClass)).OnCurIdChange();
        //refresh
    }
}
