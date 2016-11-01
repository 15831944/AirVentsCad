using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace VentsCadLibrary
{
    /// <summary>
    /// Класс реализующий работу с базой данных
    /// </summary>
    public partial class VentsCad
    {
        public void AirVents_SetPDMID(int type, int idPdm, int part)
        {
            using (var con = new SqlConnection(ConnectionToSql))
            {
                try
                {
                    con.Open();
                    var sqlCommand = new SqlCommand("AirVents.SetPDMID", con) { CommandType = CommandType.StoredProcedure };

                    sqlCommand.Parameters.AddWithValue("@Type", type);   // Если сборка = 2, Если деталь = 1
                    sqlCommand.Parameters.AddWithValue("@IDPDM", idPdm);
                    sqlCommand.Parameters.AddWithValue("@PART", part);   //@Type = 1 PartID = @PART // @Type = 2 PanelNumber = @PART

                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message + "\n" + e);
                }
                finally
                {
                    con.Close();
                }
            }
        }


        /// <summary>
        /// Panels the type identifier.
        /// </summary>
        /// <param name="panelTypeCode">The panel type code.</param>
        /// <returns></returns>
        public int PanelTypeId(string panelTypeCode)
        {
            var panelTypeId = 1;
            using (var con = new SqlConnection(ConnectionToSql))
            {
                try
                {
                    con.Open();
                    var sqlCommand =
                        new SqlCommand(@"SELECT *  FROM [AirVents].[PanelType] WHERE[PanelTypeCode]   = '" + panelTypeCode + "'", con);
                    var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                    var dataTable = new DataTable("panelTypeName");
                    sqlDataAdapter.Fill(dataTable);
                    panelTypeId = Convert.ToInt32(dataTable.Rows[0]["PanelTypeID"]);
                }
                catch (Exception)
                {
                    con.Close();
                }
                finally
                {
                    con.Close();
                }
            }
            return panelTypeId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableParams"></param>
        /// <returns></returns>
        public bool AirVents_AddPanelFull(DataTable tableParams)
        {
            using (var con = new SqlConnection(ConnectionToSql))
            {
                try
                {
                    con.Open();
                    var sqlCommand = new SqlCommand("AirVents_AddPanel", con) { CommandType = CommandType.StoredProcedure };

                    #region @Width

                    if (tableParams == null)

                    {
                        sqlCommand.Parameters.AddWithValue("@Width", DBNull.Value);
                    }
                    else
                    {
                        var sqlParameter = sqlCommand.Parameters.AddWithValue("@Width", tableParams);
                        sqlParameter.SqlDbType = SqlDbType.Structured;
                    }

                    #endregion

                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message + "\n" + e);
                    return false;
                }
                finally
                {
                    con.Close();
                }
            }
            return true;
        }

        /// <summary>
        /// Airs the vents_ add part of panel.
        /// </summary>
        /// <param name="panelTypeId">Name of the panel type.</param>
        /// <param name="elementType">Type of the element.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="partThick">The part thick.</param>
        /// <param name="partMat">The part mat.</param>
        /// <param name="partMatThick">The part mat thick.</param>
        /// <param name="mirror">The mirror.</param>
        /// <param name="step">The step.</param>
        /// <param name="stepInsertion">The step insertion.</param>
        /// <param name="reinforcing">if set to <c>true</c> [reinforcing].</param>
        /// <param name="stickyTape">if set to <c>true</c> [sticky tape].</param>
        /// <returns></returns>
        public int AirVents_AddPartOfPanel
            (
            //int? pdmId,
            int panelTypeId,
            int elementType,
            int? width,
            int? height,
            int partThick,
            int? partMat,
            double? partMatThick,

            //string ral,
            //string coatingType,
            //int? coatingClass,

            bool? mirror,
            string step,
            string stepInsertion,
            bool reinforcing,
            bool stickyTape
            )
        {
            var partId = 0;
            using (var con = new SqlConnection(ConnectionToSql))
            {

                try
                {
                    con.Open();
                    var sqlCommand = new SqlCommand("AirVents_AddPartOfPanel", con) { CommandType = CommandType.StoredProcedure };

                    #region PDM ID

                    //if (pdmId == null)
                    //{
                    //    sqlCommand.Parameters.AddWithValue("@PDMID", DBNull.Value);                        
                    //}
                    //else
                    //{
                    //    sqlCommand.Parameters.AddWithValue("@PDMID", pdmId);                        
                    //}

                    #endregion

                    sqlCommand.Parameters.AddWithValue("@PanelTypeID", panelTypeId);//@PanelTypeName = 'Ножки опорные',
                    sqlCommand.Parameters.AddWithValue("@ElementType", elementType);//@ElementType = 1,

                    #region @Width

                    if (width == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@Width", DBNull.Value);//@Width = null
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@Width", width);
                    }

                    #endregion

                    #region @Height

                    if (height == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@Height", DBNull.Value);//@Height = null
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@Height", height);
                    }

                    #endregion


                    #region @PartThick

                    sqlCommand.Parameters.AddWithValue("@PartThick", partThick);//@Thickness = 45,

                    #endregion

                    #region @PartMat

                    if (partMat == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@PartMat", DBNull.Value);//@PartMat = null
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@PartMat", partMat);
                    }

                    #endregion

                    #region @PartMatThick

                    if (partMatThick == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@PartMatThick", DBNull.Value);//@PartMatThick = null,
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@PartMatThick", partMatThick);
                    }

                    #endregion @SoundIns

                    #region @RAL

                    //if (ral == null)
                    //{
                    //    sqlCommand.Parameters.AddWithValue("@RAL", DBNull.Value);//@RAL = NULL,
                    //}
                    //else
                    //{
                    //    sqlCommand.Parameters.AddWithValue("@RAL", ral);
                    //}

                    #endregion

                    #region @CoatingType

                    //if (string.IsNullOrEmpty(coatingType))
                    //{
                    //    sqlCommand.Parameters.AddWithValue("@CoatingType", DBNull.Value); //@CoatingType = NULL,
                    //}
                    //else
                    //{
                    //    sqlCommand.Parameters.AddWithValue("@CoatingType", coatingType);
                    //}

                    #endregion

                    #region @CoatingClass

                    //if (string.IsNullOrEmpty(coatingClass.ToString()))
                    //{
                    //    sqlCommand.Parameters.AddWithValue("@CoatingClass", DBNull.Value); //@CoatingClass = NULL,
                    //}
                    //else
                    //{
                    //    sqlCommand.Parameters.AddWithValue("@CoatingClass", coatingClass);
                    //}

                    #endregion


                    #region @Mirror

                    if (mirror == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@Mirror", DBNull.Value); //@Mirror = NULL,
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@Mirror", mirror);
                    }

                    #endregion

                    #region @Step

                    if (step == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@Step", DBNull.Value); //@Step = NULL,
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@Step", step);
                    }

                    #endregion

                    #region @StepInsertion

                    if (stepInsertion == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@StepInsertion", DBNull.Value); //@StepInsertion = NULL,
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@StepInsertion", stepInsertion);
                    }

                    #endregion

                    #region @Reinforcing

                    //if (reinforcing01 == null)
                    //{
                    //    sqlCommand.Parameters.AddWithValue("@Reinforcing", DBNull.Value);//@Reinforcing = 1,
                    //}
                    //else
                    //{
                    sqlCommand.Parameters.AddWithValue("@Reinforcing", reinforcing);// Convert.ToBoolean(reinforcing01) ? 0 : 1);
                    //}

                    #endregion

                    #region @StickyTape

                    sqlCommand.Parameters.AddWithValue("@StickyTape", stickyTape);// Convert.ToBoolean(reinforcing01) ? 0 : 1);

                    #endregion

                    //sqlCommand.Parameters.AddWithValue("@PARTID", 1);
                    //var returnParameter = sqlCommand.Parameters.Add("@PARTID", SqlDbType.Int);
                    //returnParameter.Direction = ParameterDirection.ReturnValue;
                    sqlCommand.Parameters.Add("@PARTID", SqlDbType.Int);
                    sqlCommand.Parameters["@PARTID"].Direction = ParameterDirection.Output;

                    sqlCommand.ExecuteNonQuery();

                    //partId = Convert.ToInt32(returnParameter.Value);

                    partId = Convert.ToInt32(sqlCommand.Parameters["@PARTID"].Value.ToString());

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message + "\n" + e);
                    return partId;
                }
                finally
                {
                    con.Close();
                }
            }
            return partId;
        }


        /// <summary>
        /// Panels the asm identifier.
        /// </summary>
        /// <returns></returns>
        public int PanelNumber()
        {
            var panelNumber = 0;
            using (var con = new SqlConnection(ConnectionToSql))
            {
                try
                {
                    con.Open();
                    var sqlCommand =
                        new SqlCommand(@"SELECT MAX([PanelNumber]) FROM [AirVents].[PanelsAsm]", con);
                    var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                    var dataTable = new DataTable("panelTypeName");
                    sqlDataAdapter.Fill(dataTable);
                    panelNumber = Convert.ToInt32(dataTable.Rows[0][0]);
                }
                catch (Exception)
                {
                    con.Close();
                }
                finally
                {
                    con.Close();
                }
            }
            return panelNumber;
        }

        /// <summary>
        /// Airs the vents_ add panel.
        /// </summary>
        /// <param name="partId">The part identifier.</param>
        /// <param name="panelTypeId">Name of the panel type.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="panelMatOut">The panel mat out.</param>
        /// <param name="panelMatIn">The panel mat in.</param>
        /// <param name="panelThick">The panel thick.</param>
        /// <param name="panelMatThickOut">The panel mat thick out.</param>
        /// <param name="panelMatThickIn">The panel mat thick in.</param>
        /// <param name="mirror">The mirror.</param>
        /// <param name="step">The step.</param>
        /// <param name="stepInsertion">The step insertion.</param>
        /// <param name="reinforcing01">if set to <c>true</c> [reinforcing01].</param>
        /// <param name="stickyTape">if set to <c>true</c> [sticky tape].</param>
        /// <param name="panelNumber">The panel number.</param>
        /// <returns></returns>
        public int AirVents_AddPanel
            (
            int partId,
            int panelTypeId,
            int? width,
            int? height,
            int? panelMatOut,
            int? panelMatIn,
            int panelThick,
            double? panelMatThickOut,
            double? panelMatThickIn,

            //string ralOut,
            //string ralIn,
            //string coatingTypeOut,
            //string coatingTypeIn,
            //int? coatingClassOut,
            //int? coatingClassIn,

            bool? mirror,
            string step,
            string stepInsertion,
            bool reinforcing01,
            bool stickyTape,

            int panelNumber
            )
        {
            var id = 0;
            using (var con = new SqlConnection(ConnectionToSql))
            {
                try
                {
                    con.Open();
                    var sqlCommand = new SqlCommand("AirVents_AddPanel", con) { CommandType = CommandType.StoredProcedure };

                    sqlCommand.Parameters.AddWithValue("@PARTID", partId);

                    sqlCommand.Parameters.AddWithValue("@PanelTypeID", panelTypeId);//@PanelTypeName = 'Ножки опорные',

                    #region @Width

                    if (width == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@Width", DBNull.Value);//@Width = null
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@Width", width);
                    }

                    #endregion

                    #region @Height

                    if (height == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@Height", DBNull.Value);//@Height = null
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@Height", height);
                    }

                    #endregion


                    #region @PanelMatOut

                    if (panelMatOut == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@PanelMatOut", DBNull.Value);
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@PanelMatOut", panelMatOut);
                    }

                    #endregion

                    #region @PanelMatIn

                    if (panelMatIn == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@PanelMatIn", DBNull.Value);//@PartMat = null
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@PanelMatIn", panelMatIn);
                    }

                    #endregion

                    #region @PanelThick

                    sqlCommand.Parameters.AddWithValue("@PanelThick", panelThick);

                    #endregion

                    #region @PanelMatThickOut

                    if (panelMatThickOut == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@PanelMatThickOut", DBNull.Value);
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@PanelMatThickOut", panelMatThickOut);
                    }

                    #endregion 

                    #region @PanelMatThickIn

                    if (panelMatThickIn == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@PanelMatThickIn", DBNull.Value);
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@PanelMatThickIn", panelMatThickIn);
                    }

                    #endregion @SoundIns

                    #region @RALOut

                    //if (ralOut == null)
                    //{
                    //    sqlCommand.Parameters.AddWithValue("@RALOut", DBNull.Value);//@RAL = NULL,
                    //}
                    //else
                    //{
                    //    sqlCommand.Parameters.AddWithValue("@RALOut", ralOut);
                    //}

                    #endregion

                    #region @RALIn

                    //if (ralIn == null)
                    //{
                    //    sqlCommand.Parameters.AddWithValue("@RALIn", DBNull.Value);//@RAL = NULL,
                    //}
                    //else
                    //{
                    //    sqlCommand.Parameters.AddWithValue("@RALIn", ralIn);
                    //}

                    #endregion

                    #region @CoatingTypeOut

                    //if (string.IsNullOrEmpty(coatingTypeOut))
                    //{
                    //    sqlCommand.Parameters.AddWithValue("@CoatingTypeOut", DBNull.Value); 
                    //}
                    //else
                    //{
                    //    sqlCommand.Parameters.AddWithValue("@CoatingTypeOut", coatingTypeOut);
                    //}

                    #endregion

                    #region @CoatingTypeIn

                    //if (string.IsNullOrEmpty(coatingTypeIn))
                    //{
                    //    sqlCommand.Parameters.AddWithValue("@CoatingTypeIn", DBNull.Value);
                    //}
                    //else
                    //{
                    //    sqlCommand.Parameters.AddWithValue("@CoatingTypeIn", coatingTypeIn);
                    //}

                    #endregion

                    #region @CoatingClassOut

                    //if (string.IsNullOrEmpty(coatingClassOut.ToString()))
                    //{
                    //    sqlCommand.Parameters.AddWithValue("@CoatingClassOut", DBNull.Value); 
                    //}
                    //else
                    //{
                    //    sqlCommand.Parameters.AddWithValue("@CoatingClassOut", coatingClassOut);
                    //}

                    #endregion

                    #region @CoatingClassIn

                    //if (string.IsNullOrEmpty(coatingClassIn.ToString()))
                    //{
                    //    sqlCommand.Parameters.AddWithValue("@CoatingClassIn", DBNull.Value);
                    //}
                    //else
                    //{
                    //    sqlCommand.Parameters.AddWithValue("@CoatingClassIn", coatingClassIn);
                    //}

                    #endregion


                    #region @Mirror

                    if (mirror == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@Mirror", DBNull.Value); //@Mirror = NULL,
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@Mirror", mirror);
                    }

                    #endregion

                    #region @Step

                    if (step == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@Step", DBNull.Value); //@Step = NULL,
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@Step", step);
                    }

                    #endregion

                    #region @StepInsertion

                    if (stepInsertion == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@StepInsertion", DBNull.Value); //@StepInsertion = NULL,
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@StepInsertion", stepInsertion);
                    }

                    #endregion

                    #region @Reinforcing

                    //if (reinforcing01 == null)
                    //{
                    //    sqlCommand.Parameters.AddWithValue("@Reinforcing", DBNull.Value);//@Reinforcing = 1,
                    //}
                    //else
                    //{
                    sqlCommand.Parameters.AddWithValue("@Reinforcing", reinforcing01);
                    //}

                    #endregion

                    #region @StickyTape

                    sqlCommand.Parameters.AddWithValue("@StickyTape", stickyTape);

                    #endregion

                    #region @PanelNumber Output

                    sqlCommand.Parameters.AddWithValue("@PanelNumber", panelNumber);

                    //sqlCommand.Parameters.Add("@PanelNumber", SqlDbType.Int);
                    sqlCommand.Parameters["@PanelNumber"].Direction = ParameterDirection.InputOutput;
                    //var returnParameter = sqlCommand.Parameters.Add("@PanelNumber", SqlDbType.Int);
                    //returnParameter.Direction = ParameterDirection.ReturnValue;

                    #endregion

                    sqlCommand.ExecuteNonQuery();

                    id = Convert.ToInt32(sqlCommand.Parameters["@PanelNumber"].Value.ToString());

                    #region 

                    //sqlCommand.Parameters.AddWithValue("@PartNumber", "");

                    //var returnParameter = sqlCommand.Parameters.Add("@PartNumber", SqlDbType.NVarChar);
                    ////var returnParameter = sqlParameter.AddWithValue("@PARTID", Номерподбора.Text);
                    //returnParameter.Direction = ParameterDirection.ReturnValue;

                    //var returnParameter = sqlCommand.Parameters.Add("@PARTID", SqlDbType.Int);
                    //returnParameter.Direction = ParameterDirection.ReturnValue;


                    //var returnParameter = sqlCommand.Parameters.Add("@PARTID", SqlDbType.Int);
                    ////var returnParameter = sqlParameter.AddWithValue("@PARTID", Номерподбора.Text);
                    //returnParameter.Direction = ParameterDirection.ReturnValue;

                    //var Id =  sqlCommand.ExecuteNonQuery();

                    //var asfv = sqlCommand.ExecuteScalar();

                    //MessageBox.Show(asfv == null ? "Null" : "Not null" + asfv.GetType());



                    //MessageBox.Show(((int)(decimal)sqlCommand.ExecuteScalar()).ToString());
                    //   MessageBox.Show(sqlCommand.ExecuteReader().GetInt32(0).ToString(),"1");

                    // MessageBox.Show(sqlCommand.ExecuteReader().GetInt32(0).ToString(), "2");


                    //reader = sqlCommand.ExecuteReader();

                    //MessageBox.Show(reader.FieldCount.ToString());


                    //if (reader.HasRows)
                    //{
                    //    while (reader.Read())
                    //    {
                    //        MessageBox.Show(reader.GetInt32(0).ToString(), reader.GetString(1));
                    //    }
                    //}
                    //else
                    //{
                    //    MessageBox.Show("No rows found.");
                    //}
                    //reader.Close();

                    // partId = Convert.ToInt32(returnParameter.Value);

                    //MessageBox.Show(returnParameter.Value.ToString());

                    #endregion

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message + "\n" + e);
                }
                finally
                {
                    con.Close();
                }
            }
            return id;
        }

        /// <summary>
        /// Airs the vents_ add panel asm.
        /// </summary>
        /// <param name="partIds">The part ids.</param>
        /// <param name="partId">The part identifier.</param>
        /// <returns></returns>
        public bool AirVents_AddPanelAsm (List<int> partIds, out int partId)
        {
            using (var con = new SqlConnection(ConnectionToSql))
            {
                partId = 0;
                try
                {
                    con.Open();
                    var sqlCommand = new SqlCommand("AirVents_AddPanel", con) { CommandType = CommandType.StoredProcedure };

                    sqlCommand.Parameters.AddWithValue("@SerialNum", "");// Convert.ToSingle(PartId));

                    var returnParameter = sqlCommand.Parameters.Add("@PARTID", SqlDbType.Int);
                    //var returnParameter = sqlParameter.AddWithValue("@PARTID", Номерподбора.Text);
                    returnParameter.Direction = ParameterDirection.ReturnValue;

                    sqlCommand.ExecuteNonQuery();

                    partId = Convert.ToInt32(returnParameter.Value);
                    //MessageBox.Show(returnParameter.Value.ToString());
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message + "\n" + e);
                    return false;
                }
                finally
                {
                    con.Close();
                }
            }
            return true;
        }

        /// <summary>
        /// Добавление панели или ее элемента в базу.
        /// </summary>
        /// <param name="panelTypeId">Имя типа панели</param><example>Ножки опорные</example>
        /// <param name="elementType">Номер типа элемента</param><example>0 - Сборка, 1 - Внешняя панель</example>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="thickness">The partThick.</param>
        /// <param name="matOut">The mat out.</param>
        /// <param name="matInt">The mat int.</param>
        /// <param name="thicknessOut">The partThick out.</param>
        /// <param name="thicknessInt">The partThick int.</param>
        /// <param name="soundIns">The sound ins.</param>
        /// <param name="stickyTape">The sticky tape.</param>
        /// <param name="reinforcing01">The reinforcing01.</param>
        /// <param name="ral">The ral.</param>
        /// <param name="coatingType">Type of the coating.</param>
        /// <param name="coatingClass">The coating class.</param>
        /// <param name="mirror">The mirror.</param>
        /// <param name="step">The step.</param>
        /// <param name="stepInsertion">The step insertion.</param>
        /// <returns></returns>
        public bool AirVents_AddPanel2(
            int panelTypeId,
            int elementType,
            int? width,
            int? height,  // 880
            int thickness,  //40 50 
            int? matOut,
            int? matInt,
            double? thicknessOut,
            double? thicknessInt,
            string soundIns,
            string stickyTape,
            bool? reinforcing01,
            string ral,
            string coatingType,
            int? coatingClass,
            bool? mirror,
            string step,
            string stepInsertion)
        {
            using (var con = new SqlConnection(ConnectionToSql))
            {
                try
                {
                    con.Open();
                    var sqlCommand = new SqlCommand("AirVents_AddPanel", con) { CommandType = CommandType.StoredProcedure };

                    sqlCommand.Parameters.AddWithValue("@PanelTypeID", panelTypeId);//@PanelTypeName = 'Ножки опорные',
                    sqlCommand.Parameters.AddWithValue("@ElementType", elementType);//@ElementType = 1,

                    #region @Width

                    if (width == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@Width", DBNull.Value);//@Width = null
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@Width", width);
                    }

                    #endregion

                    #region @Height

                    if (height == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@Height", DBNull.Value);//@Height = null
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@Height", height);
                    }

                    #endregion

                    sqlCommand.Parameters.AddWithValue("@Thickness", thickness);//@Thickness = 45,

                    #region @MatOut

                    if (matOut == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@MatOut", DBNull.Value);//@MatOut = null
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@MatOut", matOut);
                    }

                    #endregion

                    #region @MatInt

                    if (matInt == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@MatInt", DBNull.Value);//@MatInt = null
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@MatInt", matInt);
                    }

                    #endregion

                    #region @ThicknessOut

                    if (thicknessOut == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@ThicknessOut", DBNull.Value);//@ThicknessOut = null,

                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@ThicknessOut", thicknessOut);
                    }

                    #endregion 

                    #region @ThicknessInt

                    if (thicknessInt == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@ThicknessInt", DBNull.Value);//@ThicknessInt = null,
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@ThicknessInt", thicknessInt);
                    }

                    #endregion @SoundIns

                    #region @SoundIns

                    if (soundIns == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@SoundIns", DBNull.Value);//@SoundIns = 'Ножки опорные',
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@SoundIns", soundIns);
                    }

                    #endregion

                    #region @StickyTape

                    if (stickyTape == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@StickyTape", DBNull.Value);//@StickyTape = 1,
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@StickyTape", stickyTape);
                    }

                    #endregion

                    #region @Reinforcing

                    if (reinforcing01 == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@Reinforcing", DBNull.Value);//@Reinforcing = 1,
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@Reinforcing", Convert.ToBoolean(reinforcing01) ? 0 : 1);
                    }

                    #endregion

                    #region @RAL

                    if (ral == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@RAL", DBNull.Value);//@RAL = NULL,
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@RAL", ral);
                    }

                    #endregion

                    #region @CoatingType

                    if (coatingType == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@CoatingType", DBNull.Value); //@CoatingType = NULL,
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@CoatingType", coatingType);
                    }

                    #endregion

                    #region @CoatingClass

                    if (coatingClass == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@CoatingClass", DBNull.Value); //@CoatingClass = NULL,
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@CoatingClass", coatingClass);
                    }

                    #endregion

                    #region @Mirror

                    if (mirror == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@Mirror", DBNull.Value); //@Mirror = NULL,
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@Mirror", mirror);
                    }

                    #endregion

                    #region @Step

                    if (step == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@Step", DBNull.Value); //@Step = NULL,
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@Step", step);
                    }

                    #endregion

                    #region @StepInsertion

                    if (stepInsertion == null)
                    {
                        sqlCommand.Parameters.AddWithValue("@StepInsertion", DBNull.Value); //@StepInsertion = NULL,
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("@StepInsertion", stepInsertion);
                    }

                    #endregion

                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message + "\n" + e);
                    return false;
                }
                finally
                {
                    con.Close();
                }
            }
            return true;
        }
        

        /// <summary>
        /// Panelses the table.
        /// </summary>
        /// <param name="panelGroup">The panel group.</param>
        /// <returns></returns>
        public DataTable PanelsTable(string panelGroup)
        {
            var panelsTable = new DataTable();
            var connectionString = ConnectionToSql;
            var query =
                "SELECT [PanelTypeName] ,[PanelTypeCode]  FROM [AirVents].[PanelType] WHERE[PanelTypeGroup] = '" + panelGroup + "'";

            var sqlConnection = new SqlConnection(connectionString);
            var sqlCommand = new SqlCommand(query, sqlConnection);
            sqlConnection.Open();
            var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            sqlDataAdapter.Fill(panelsTable);
            sqlConnection.Close();
            sqlDataAdapter.Dispose();
            //panelsTable.Columns[0].ColumnName = "Группа";
            return panelsTable;
        }

        public int PanelsTypeId(string panelTypeCode)
        {
            var panelsTable = new DataTable();
            var connectionString = ConnectionToSql;
            var query =
                "SELECT [PanelTypeID] FROM [AirVents].[PanelType] WHERE[PanelTypeCode] = '" + panelTypeCode + "'";
            var sqlConnection = new SqlConnection(connectionString);
            var sqlCommand = new SqlCommand(query, sqlConnection);
            sqlConnection.Open();
            var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            sqlDataAdapter.Fill(panelsTable);
            sqlConnection.Close();
            sqlDataAdapter.Dispose();
            //MessageBox.Show(panelsTable.Rows[0][0].ToString());           
            return Convert.ToInt32(panelsTable.Rows[0][0].ToString());
        }


        /// <summary>
        /// Panelses the table.
        /// </summary>
        /// <returns></returns>
        public DataTable PanelsTable()
        {
            var panelsTable = new DataTable();
            var connectionString = ConnectionToSql;
            const string query = @"SELECT * FROM [SWPlusDB].[AirVents].[PanelType]
WHERE PanelTypeGroup = 2
Order BY PanelTypeCode";
            var sqlConnection = new SqlConnection(connectionString);
            var sqlCommand = new SqlCommand(query, sqlConnection);
            sqlConnection.Open();
            var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            sqlDataAdapter.Fill(panelsTable);
            sqlConnection.Close();
            sqlDataAdapter.Dispose();
            //panelsTable.Columns[0].ColumnName = "Группа";
            return panelsTable;
        }

        /// <summary>
        /// Materialses the table.
        /// </summary>
        /// <returns></returns>
        public DataTable MaterialsTable()
        {
            var materialsTable = new DataTable();
            try
            {
                var connectionString = ConnectionToSql;
                const string query = @"SELECT [LevelID], CodeMaterial, MaterialsName
        FROM  [dbo].[MaterialsProp]
        WHERE  [Thickness] = 1";
                var sqlConnection = new SqlConnection(connectionString);
                var sqlCommand = new SqlCommand(query, sqlConnection);
                sqlConnection.Open();
                var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                sqlDataAdapter.Fill(materialsTable);
                sqlConnection.Close();
                sqlDataAdapter.Dispose();
            }
            catch (Exception)
            {
                materialsTable = null;
            }

            return materialsTable;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DataTable MaterialsForMontageFrame()
        {
            var materialsTable = new DataTable();
            var connectionString = ConnectionToSql;
            const string query = @"SELECT [LevelID], CodeMaterial, MaterialsName
        FROM  [dbo].[MaterialsProp]
        WHERE levelid IN (1800, 2100, 18400)";
            var sqlConnection = new SqlConnection(connectionString);
            var sqlCommand = new SqlCommand(query, sqlConnection);
            sqlConnection.Open();
            var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            sqlDataAdapter.Fill(materialsTable);
            sqlConnection.Close();
            sqlDataAdapter.Dispose();
            return materialsTable;
        }


        /// <summary>
        /// Таблица свойств материалов, возвращаемая из базы
        /// </summary>
        /// <param name="material">The material - имя или код материал по базе</param>
        /// <returns> DataTable</returns>
        public DataTable MaterialsTable(string material)
        {
            var materialsTable = new DataTable();
            var connectionString = ConnectionToSql;
            var query = "select * from MaterialsProp";
            if (material != "")
            {
                try
                {
                    var int32 = Convert.ToInt32(material);
                    //MessageBox.Show(int32.ToString());
                    query = "select * from MaterialsProp where LevelID = " + int32 + "";
                }
                catch
                {
                    try
                    {
                        query = "select * from MaterialsProp where CodeMaterial = '" + material + "'";
                    }
                    catch (Exception)
                    {
                        query = "select * from MaterialsProp where MaterialsName = '" + material + "'";
                    }
                }

            }

            var sqlConnection = new SqlConnection(connectionString);
            var sqlCommand = new SqlCommand(query, sqlConnection);
            sqlConnection.Open();
            var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            sqlDataAdapter.Fill(materialsTable);
            sqlConnection.Close();
            sqlDataAdapter.Dispose();
            return materialsTable;
        }

        /// <summary>
        /// Filterses the table.
        /// </summary>
        /// <param name="sizeType">Type of the size.</param>
        /// <returns></returns>
        public DataTable FiltersTable(string sizeType)
        {
            var filtersTable = new DataTable("FiltersTable");
            var idSize = "";
            using (var con = new SqlConnection(ConnectionToSql))
            {
                try
                {
                    con.Open();
                    var sqlCommand =
                        new SqlCommand("SELECT SizeID FROM StandardSize Where Type = '" + sizeType + "'", con);
                    var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                    var dataTable = new DataTable("StandartSize");
                    sqlDataAdapter.Fill(dataTable);
                    idSize = dataTable.Rows[0]["SizeID"].ToString();
                    sqlDataAdapter.Dispose();
                    con.Close();
                }
                catch (Exception)
                {
                    con.Close();
                }

                try
                {
                    con.Open();
                    var sqlCommand =
                        new SqlCommand("SELECT * FROM AirVentsHeaterExchander Where IDSize = '" + idSize + "'", con);
                    var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                    sqlDataAdapter.Fill(filtersTable);
                    sqlDataAdapter.Dispose();
                    con.Close();
                }
                catch (Exception)
                {
                    con.Close();
                }
            }
            return filtersTable;
        }


        /// <summary>
        /// Table of standard sizes.
        /// </summary>
        /// <returns></returns>
        public DataTable AirVentsStandardSize()
        {
            var standartSizeTable = new DataTable();//("StandardSize");
            using (var con = new SqlConnection(ConnectionToSql))
            {
                try
                {
                    con.Open();
                    var sqlCommand = new SqlCommand("SELECT * FROM  AirVents.StandardSize  ORDER BY Type", con);
                    var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                    sqlDataAdapter.Fill(standartSizeTable);
                    sqlDataAdapter.Dispose();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Ошибка выгрузки данных из базы");
                }
                finally
                {
                    con.Close();
                }
            }
            return standartSizeTable;
        }
             

        /// <summary>
        /// Возвращает стандарнтое сечение для установки
        /// </summary>
        /// <param name="panelTypeCode">типоразмер установки</param>
        /// <returns>Ширина, Высота</returns>
        public string[] StandartSize(string panelTypeCode)
        {

            const string width = "500";//870//650
            const string height = "500";
            //using (SqlConnection con = new SqlConnection(Properties.Settings.Default.ConnectionToSQL))
            //using (var con = new SqlConnection(Properties.Settings.Default.ConnectionToSQL))
            //{
            //    try
            //    {
            //        con.Open();
            //        var sqlCommand =
            //            new SqlCommand("SELECT Wight, Hight FROM StandardSize Where Type = '" + sizeType + "'", con);
            //        var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            //        var dataTable = new DataTable("StandartSize");
            //        sqlDataAdapter.Fill(dataTable);
            //        width = dataTable.Rows[0]["Wight"].ToString();
            //        height = dataTable.Rows[0]["Hight"].ToString();
            //    }
            //    catch (Exception)
            //    {
            //        con.Close();
            //    }
            //    finally
            //    {
            //        con.Close();
            //    }
            //}

            return new[] { width, height };

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sizeId"></param>
        /// <param name="profilId"></param>
        /// <returns></returns>
        public string[] StandartSize(int sizeId, int profilId)
        {
            string width = null;
            string height = null;
            using (var con = new SqlConnection(ConnectionToSql))
            {
                try
                {
                    con.Open();
                    var sqlCommand =
                        new SqlCommand(@"SELECT
AirVents.StandardSize.Type,
AirVents.Profil.Description,
AirVents.Dimension.Wight,
AirVents.Dimension.Hight
FROM AirVents.DimensionType
INNER JOIN AirVents.Dimension
  ON AirVents.DimensionType.DimensionID = AirVents.Dimension.DimensionID
INNER JOIN AirVents.StandardSize
  ON AirVents.DimensionType.SizeID = AirVents.StandardSize.SizeID
INNER JOIN AirVents.Profil
  ON AirVents.DimensionType.ProfilID = AirVents.Profil.ProfilID
  WHERE AirVents.StandardSize.SizeID = " + sizeId + " AND  AirVents.Profil.ProfilID = " + profilId, con);
                    var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                    var dataTable = new DataTable("StandartSize");
                    sqlDataAdapter.Fill(dataTable);
                    width = dataTable.Rows[0]["Wight"].ToString();
                    height = dataTable.Rows[0]["Hight"].ToString();
                }
                catch (Exception)
                {
                    con.Close();
                }
                finally
                {
                    con.Close();
                }
            }

            return new[] { width, height };
        }

        /// <summary>
        /// Параметры гибки листового металла 
        /// </summary>
        /// <param name="thikness">толщина материала</param>
        /// <returns>Радиус гиба, Kfactor </returns>
        public string[] BendTable(string thikness)
        {
            var radius = "1";
            var kFactor = "1";
            using (var sqlConnection = new SqlConnection(ConnectionToSql))
            {
                try
                {
                    sqlConnection.Open();
                    var sqlCommand =
                        new SqlCommand(
                            "SELECT [K-Factor], BendRadius FROM Materials.BendTable Where Thickness = " +
                            thikness.Replace(',', '.'), sqlConnection);
                    var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                    var dataTable = new DataTable("BendTable");
                    sqlDataAdapter.Fill(dataTable);
                    radius = dataTable.Rows[0]["BendRadius"].ToString();
                    kFactor = dataTable.Rows[0]["K-Factor"].ToString();
                    sqlConnection.Close();
                }
                catch (Exception)
                {
                    sqlConnection.Close();
                }

            }
            //swApp.SendMsgToUser(Radius + "  " + KFactor);
            return new[] { radius, kFactor };
        }

        #region Adminka

        /// <summary>
        /// Добавляет пользователя в базу SQL.
        /// </summary>
        /// <param name="userName">Имя пользователя</param>
        /// <param name="fullName">Полное имя пользователя</param>
        /// <returns>true - если успешно</returns>
        public bool AddUserToSqlBase(string userName, string fullName)
        {
            using (var con = new SqlConnection(ConnectionToSql))
            {
                try
                {
                    con.Open();
                    var sqlCommand =
                        new SqlCommand("INSERT into Users (Username, FullName) VALUES " + "('" + userName + "','" + fullName + "')", con);
                    var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                    var dataTable = new DataTable("Username");
                    sqlDataAdapter.Fill(dataTable);
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    con.Close();
                }
            }
            return true;
        }


        /// <summary>
        /// Удаляет пользователя из базы SQL.
        /// </summary>
        /// <param name="userName">Имя пользователя</param>
        /// <returns>true - если успешно</returns>
        public bool DelUserFromSqlBase(string userName)
        {
            using (var con = new SqlConnection(ConnectionToSql))
            {
                try
                {
                    con.Open();
                    var sqlCommand =
                        new SqlCommand("DELETE from Users WHERE (Username) = " + "('" + userName + "')", con);
                    var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                    var dataTable = new DataTable("Username");
                    sqlDataAdapter.Fill(dataTable);
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    con.Close();
                }
            }
            return true;
        }

        /// <summary>
        /// Добавление группы в базу SQL.
        /// </summary>
        /// <param name="groupname">Имя группы.</param>
        /// <param name="description">Описание группы.</param>
        /// <returns>true - если успешно</returns>
        public bool AddGroupToSqlBase(string groupname, string description)
        {
            using (var con = new SqlConnection(ConnectionToSql))
            {
                try
                {
                    con.Open();
                    var sqlCommand =
                        new SqlCommand("INSERT into Groups (Groupname, Description) VALUES " + "('" + groupname + "','" + description + "')", con);
                    var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                    var dataTable = new DataTable("Groupname");
                    sqlDataAdapter.Fill(dataTable);
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    con.Close();
                }
            }
            return true;
        }

        /// <summary>
        /// Добавление пользователя в группу быазы SQL.
        /// </summary>
        /// <param name="userName">Имя пользователя.</param>
        /// <param name="fullName">Полное имя пользователя.</param>
        /// <param name="groupname">Имя группы.</param>
        /// <returns>true - если успешно</returns>
        public bool AddUserToGroupInSqlBase(string userName, string fullName, string groupname)
        {
            using (var con = new SqlConnection(ConnectionToSql))
            {
                try
                {
                    con.Open();
                    var sqlCommand = new SqlCommand("ADDUSERGROUP", con) { CommandType = CommandType.StoredProcedure };
                    sqlCommand.Parameters.Add("@Username", SqlDbType.NVarChar).Value = userName;
                    sqlCommand.Parameters.Add("@FullName", SqlDbType.NVarChar).Value = fullName;
                    //sqlCommand.Parameters.Add("@GroupName", SqlDbType.NVarChar).Value = groupname;
                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    con.Close();
                }
            }
            return true;
        }


        /// <summary>
        /// Adds the user in SQL base.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
        public bool AddUserInSqlBase(string userName, string fullName)
        {
            using (var con = new SqlConnection(ConnectionToSql))
            {
                try
                {
                    con.Open();
                    var sqlCommand = new SqlCommand("AddUser", con) { CommandType = CommandType.StoredProcedure };
                    sqlCommand.Parameters.Add("@Username", SqlDbType.NVarChar).Value = userName;
                    sqlCommand.Parameters.Add("@FullName", SqlDbType.NVarChar).Value = fullName;
                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    con.Close();
                }
            }
            return true;
        }


        /// <summary>
        /// Удаление пользователя из группы SQL.
        /// </summary>
        /// <param name="userName">Имя пользователя.</param>
        /// <param name="groupname">Имя группы.</param>
        /// <returns>true - если успешно</returns>
        public bool DeleteUserFromGroup(string userName, string groupname)
        {
            using (var con = new SqlConnection(ConnectionToSql))
            {
                try
                {
                    con.Open();
                    var sqlCommand = new SqlCommand("DeleteUserFromGroup", con)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    sqlCommand.Parameters.Add("@Username", SqlDbType.NVarChar).Value = userName;
                    sqlCommand.Parameters.Add("@GroupName", SqlDbType.NVarChar).Value = groupname;
                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    con.Close();
                }
            }
            return true;
        }

        /// <summary>
        /// Удаление группы из базы SQL.
        /// </summary>
        /// <param name="groupname">Имя группы.</param>
        /// <returns>true - если успешно</returns>
        public bool DeleteGroupToSqlBase(string groupname)
        {
            using (var con = new SqlConnection(ConnectionToSql))
            {
                try
                {
                    con.Open();
                    var sqlCommand =
                        new SqlCommand("DELETE from Groups WHERE (Groupname) = " + "('" + groupname + "')", con);
                    var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                    var dataTable = new DataTable("Groupname");
                    sqlDataAdapter.Fill(dataTable);
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    con.Close();
                }
            }
            return true;
        }

        /// <summary>
        /// Таблица с группамы из базы SQL.
        /// </summary>
        /// <returns>Таблица с группамы из базы SQL.</returns>
        public DataTable GroupsTable()
        {
            var groupsTable = new DataTable();
            var connectionString = ConnectionToSql;
            const string query = "select Groupname from Groups";

            var sqlConnection = new SqlConnection(connectionString);
            var sqlCommand = new SqlCommand(query, sqlConnection);
            sqlConnection.Open();
            var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            sqlDataAdapter.Fill(groupsTable);
            sqlConnection.Close();
            sqlDataAdapter.Dispose();
            groupsTable.Columns[0].ColumnName = "Группа";
            return groupsTable;
        }

        /// <summary>
        /// Таблица с группамы из базы SQL.
        /// </summary>
        /// <param name="groupname">Имя группы.</param>
        /// <returns>Таблица с группамы из базы SQL.</returns>
        public DataTable GroupUsers(string groupname)
        {
            //Groupname = "Administrator";
            var groupUsers = new DataTable();
            var connectionString = ConnectionToSql;
            var query = "SELECT     Users.Username, Users.FullName from Groups " +
                           "INNER JOIN GroupMembers ON Groups.GroupID = GroupMembers.IDGorup INNER JOIN " +
                           "Users ON GroupMembers.IDUser = Users.UserID " +
                           "where Groups.Groupname = '" + groupname + "'";

            var sqlConnection = new SqlConnection(connectionString);
            var sqlCommand = new SqlCommand(query, sqlConnection);
            sqlConnection.Open();
            var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            sqlDataAdapter.Fill(groupUsers);
            sqlConnection.Close();
            sqlDataAdapter.Dispose();
            groupUsers.Columns[0].ColumnName = "Учетная запись";
            groupUsers.Columns[1].ColumnName = "Полное имя";
            return groupUsers;
        }

        #endregion

    }

}