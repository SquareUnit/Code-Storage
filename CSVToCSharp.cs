using System.Collection;
using System.Collection.Generic;
using UnityEngine;
using System.IO;

public class ReadCSV : Monobehaviour {
  
  private void Start() 
  {
    ReadCSVFile();
  }
  
  private void ReadCVS() 
  {
    StreamReader strReader = new StreamReader("D:\\UnityProject\\CSVParser\\PlayerData.csv");
    bool endOfFile = false;
    while(!endOfFile) 
    {
      string dataString = strReader.ReadLine();
      if(dataString == null) 
      {
        endOfFile = true;
        break;
      }
      
      var dataValues = dataString.Split(',');
      for(int i = 0; i < dataValues.Length; i++) 
      {
        Debug.Log("Value : " + i.ToString() + " " + dataValues[i].ToString());
      }
      //  Debug.Log(dataValues[0].ToString() + " " + dataValues[1].ToString() + " " + dataValues[2].ToString() + " " + dataValues[3].ToString());
    }
  }
}
