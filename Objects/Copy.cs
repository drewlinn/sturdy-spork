using System.Collections.Generic;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Library
{
  public class Copy
  {
    private int _id;
    private string _name;
    private int _inStock;
    private int _checkedOut;

    public Copy(string name, int inStock, int checkedOut, int id = 0)
    {
      _name = name;
      _inStock = inStock;
      _checkedOut = checkedOut;
      _id = id;
    }

    public int GetId()
    {
      return _id;
    }
    public string GetName()
    {
      return _name;
    }
    public int GetInStock()
    {
      return _inStock;
    }
    public int GetCheckedOut()
    {
      return _checkedOut;
    }

    public override bool Equals(System.Object otherCopy)
    {
      if(!(otherCopy is Copy))
      {
        return false;
      }
      else
      {
        Copy newCopy = (Copy) otherCopy;
        bool idEquality = (this.GetId() == newCopy.GetId());
        bool nameEquality = (this.GetName() == newCopy.GetName());
        bool inStockEquality = (this.GetInStock() == newCopy.GetInStock());
        bool checkedOutEquality = (this.GetCheckedOut() == newCopy.GetCheckedOut());
        return (idEquality && nameEquality && inStockEquality && checkedOutEquality);
      }
    }

    public override int GetHashCode()
    {
      return this.GetName().GetHashCode();
    }

    public static List<Copy> GetAll()
    {
      List<Copy> AllCopy = new List<Copy>{};
      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlCommand cmd = new SqlCommand("SELECT * FROM copies", conn);
      SqlDataReader rdr = cmd.ExecuteReader();
      while(rdr.Read())
      {
        int id = rdr.GetInt32(0);
        string name = rdr.GetString(1);
        int inStock = rdr.GetInt32(2);
        int checkedOut = rdr.GetInt32(3);
        Copy newCopy = new Copy(name, inStock, checkedOut, id);
        AllCopy.Add(newCopy);
      }
      if (rdr != null)
      {
       rdr.Close();
      }
      if (conn != null)
      {
       conn.Close();
      }
      return AllCopy;
    }

    public void Save()
    {
      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlCommand cmd = new SqlCommand("INSERT INTO copies (name, in_stock, checked_out) OUTPUT INSERTED.id VALUES (@name, @inStock, @checkedOut);", conn);

      SqlParameter namePara = new SqlParameter("@name", this.GetName());
      SqlParameter inStockPara = new SqlParameter("@inStock", this.GetInStock());
      SqlParameter checkedOut = new SqlParameter("@checkedOut", this.GetCheckedOut());

      cmd.Parameters.Add(namePara);
      cmd.Parameters.Add(inStockPara);
      cmd.Parameters.Add(checkedOut);

      SqlDataReader rdr = cmd.ExecuteReader();

      while(rdr.Read())
      {
        this._id = rdr.GetInt32(0);
      }
      if(rdr != null)
      {
        rdr.Close();
      }
      if(conn != null)
      {
        conn.Close();
      }
    }

    public static Copy Find(int id)
    {
      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlCommand cmd = new SqlCommand("SELECT * FROM copies WHERE id = @id;", conn);
      SqlParameter idParameter = new SqlParameter("@id", id.ToString());

      cmd.Parameters.Add(idParameter);
      SqlDataReader rdr = cmd.ExecuteReader();

      int foundId = 0;
      string name = null;
      int inStock = 0;
      int checkedOut = 0;

      while(rdr.Read())
      {
        foundId = rdr.GetInt32(0);
        name = rdr.GetString(1);
        inStock = rdr.GetInt32(2);
        checkedOut = rdr.GetInt32(3);
      }
      Copy foundCopy = new Copy(name, inStock, checkedOut, foundId);
      if (rdr != null)
      {
        rdr.Close();
      }
      if (conn != null)
      {
        conn.Close();
      }
      return foundCopy;
    }
    //
    public void Update(string name, int inStock, int checkedOut)
    {
      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlCommand cmd = new SqlCommand("UPDATE copies SET name = @name, checked_out = @checkedOut, in_stock = @inStock WHERE id = @Id;", conn);

      SqlParameter namePara = new SqlParameter("@name", name);
      SqlParameter inStockPara = new SqlParameter("@inStock", inStock);
      SqlParameter checkedOutPara = new SqlParameter("@checkedOut", checkedOut);
      SqlParameter idPara = new SqlParameter("@Id", this.GetId());

      cmd.Parameters.Add(namePara);
      cmd.Parameters.Add(inStockPara);
      cmd.Parameters.Add(checkedOutPara);
      cmd.Parameters.Add(idPara);

      this._name = name;
      this._inStock = inStock;
      this._checkedOut = checkedOut;
      cmd.ExecuteNonQuery();
      conn.Close();
    }


    public List<Patron> GetPatrons()
    {
      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlCommand cmd = new SqlCommand("SELECT patrons.* FROM copies JOIN patrons_copies ON (copies.id = patrons_copies.copy_id) JOIN patrons ON (patrons_copies.patron_id = patrons.id) WHERE copies.id = @CopysId;", conn);
      SqlParameter CopysIdParam = new SqlParameter("@CopysId", this.GetId().ToString());

      cmd.Parameters.Add(CopysIdParam);

      SqlDataReader rdr = cmd.ExecuteReader();

      List<Patron> patrons = new List<Patron>{};

      while(rdr.Read())
      {
        int patronId = rdr.GetInt32(0);
        string name = rdr.GetString(1);
        Patron newPatron = new Patron(name, patronId);
        patrons.Add(newPatron);
      }

      if (rdr != null)
      {
        rdr.Close();
      }
      if (conn != null)
      {
        conn.Close();
      }
      return patrons;
    }
    //Add copy's id and patron's id to patrons_copies table
    public void AddPatron(Patron newPatron)
    {
      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlCommand cmd = new SqlCommand("INSERT INTO patrons_copies (patron_id, copy_id) VALUES (@PatronId, @CopyId);", conn);

      SqlParameter copyIdParameter = new SqlParameter("@CopyId", this.GetId());
      SqlParameter patronIdParameter = new SqlParameter( "@PatronId", newPatron.GetId());

      cmd.Parameters.Add(copyIdParameter);
      cmd.Parameters.Add(patronIdParameter);
      cmd.ExecuteNonQuery();
      if (conn != null)
      {
        conn.Close();
      }
    }

    // public void Delete()
    // {
    //   SqlConnection conn = DB.Connection();
    //   conn.Open();
    //
    //   SqlCommand cmd = new SqlCommand("DELETE FROM copies WHERE id = @copyId; DELETE FROM patrons_copies WHERE copy_id = @copyId;", conn);
    //   SqlParameter copyIdParameter = new SqlParameter("@copyId", this.GetId());
    //
    //   cmd.Parameters.Add(copyIdParameter);
    //   cmd.ExecuteNonQuery();
    //
    //   if (conn != null)
    //   {
    //    conn.Close();
    //   }
    // }

    public static void DeleteAll()
    {
     SqlConnection conn = DB.Connection();
     conn.Open();
     SqlCommand cmd = new SqlCommand("DELETE FROM copies;", conn);
     cmd.ExecuteNonQuery();
     conn.Close();
    }
  }
}
