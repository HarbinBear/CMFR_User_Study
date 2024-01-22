using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Framework.CMFR
{
    public class TextViewController : MonoBehaviour, IController
    {

        public TextMeshProUGUI textComp;

        private void Start()
        {
            UpdateText();
        }

        // 函数用于添加一行新文本
        public void AddTextLine(string newLine)
        {
            textComp.text += newLine + "\n"; // "\n" 是换行符，它将确保每次添加的新内容是新的一行
        }

        public string MakeLine<T>(string name, T value) 
        {
            string newLine = name + " : " +  value;

            return newLine; 
        }

        // public void MakeLineFromModel<T>(string name)
        public void MakeLineFromModel(string name)
        {
            PropertyInfo info = typeof(ICMFRModel).GetProperty(name);
            if (info != null)
            {
                object val = info.GetValue(CMFRDemo.Interface.GetModel<ICMFRModel>() );
                // T value = val; 
                string line = MakeLine(name, val);
                AddTextLine( line );
            }
        }

        public void UpdateText()
        {
            MakeLineFromModel("sigma");
            MakeLineFromModel("fx");
            MakeLineFromModel("fy");
            MakeLineFromModel("eyeX");
            MakeLineFromModel("eyeY");
            
        } 
        
        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return CMFRDemo.Interface;
        }
        
    }
}