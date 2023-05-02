using eLiDAR.Helpers;
using eLiDAR.Models;
using eLiDAR.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace eLiDAR.API
{
    // main class to control the API to the REST end point and push and pull data from Azure SQL database
    class SynchManager
    {
        readonly DatabaseHelper databasehelper;
        readonly RestService service;
        readonly SETTINGS settings;
        readonly Utils util = new Utils();
        private DateTime prevdate;
        private DateTime prevsynchdate;
        private int plotpulls;
        private int projectpulls;
        private int treepulls;
        private int plotpushes;
        private int projectpushes;
        private int treepushes;

        public SynchManager()
        {
            service = new RestService(); 
            databasehelper = new DatabaseHelper();
            settings = databasehelper.GetSettingsData();
            prevdate = GetDate();
             
        }
        // main procedure from which to run the synch process
        // all other sych processes start form here
        //  there is one set of procedires for each of the 10 tables

        public async Task<bool> RunLoad()
        {
            try
            {
                DateTime basedate = new DateTime(2000, 1, 1, 0, 0, 0);
                //confirm the date used for the last synch
                if (DateTime.Compare(settings.LastSynched, basedate) < 0)
                {
                    prevdate = basedate;
                    settings.LastSynched = basedate;
                }

                // execute the synch on each table here
                Task<bool> doproject = ProjectSynch(true);
                bool projectdone = await doproject;

                Task<bool> doperson = PersonSynch(true);
                bool persondone = await doperson;

                Task<bool> doplot = PlotSynch(true);
                bool plotdone = await doplot;


                // finish up by saving the details
                if (projectdone && plotdone && persondone  && service.IsSuccess)
                {
                    settings.LastSynched = DateTime.UtcNow;
                    settings.PROJECT_ROWS_SYNCHED = projectpushes;
                    settings.PLOT_ROWS_SYNCHED = plotpushes;
                    settings.TREE_ROWS_SYNCHED = treepushes;
                    settings.PROJECT_ROWS_PULLED = projectpulls;
                    settings.PLOT_ROWS_PULLED = plotpulls;
                    settings.TREE_ROWS_PULLED = treepulls;

                    databasehelper.UpdateSettings(settings);
                    return true;
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Synch did not finish", service.Msg, "Ok");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogWriter logger = new LogWriter();
                logger.LogWrite(ex.Message);
                return false;
            }
        }

        public async Task<bool> RunSynch()
        {
            try
            {
                DateTime basedate = new DateTime(2000, 1, 1, 0, 0, 0);
                //confirm the date used for the last synch
                if (DateTime.Compare(settings.LastSynched, basedate) < 0)
                {
                    prevdate = basedate;
                    settings.LastSynched = basedate;
                }
                // execute the synch on each table here
                Task<bool> doproject = ProjectSynch();
                bool projectdone = await doproject;

                Task<bool> doperson = PersonSynch();
                bool persondone = await doperson;

                Task<bool> doplot = PlotSynch();
                bool plotdone = await doplot;

                Task<bool> dotree = TreeSynch();
                bool treedone = await dotree;

                Task<bool> dostemmap = StemmapSynch();
                bool stemmapdone = await dostemmap;

                Task<bool> doecosite = EcositeSynch();
                bool ecositedone = await doecosite;

                Task<bool> dosoil = SoilSynch();
                bool soildone = await dosoil;

                Task<bool> dosmalltree = SmalltreeSynch();
                bool smalltreedone = await dosmalltree;

                Task<bool> dosmalltreetally = SmalltreeTallySynch();
                bool smalltreetallydone = await dosmalltreetally;

                Task<bool> dovegetation = VegetationSynch();
                bool vegetationdone = await dovegetation;

                Task<bool> dodeformity = DeformitySynch();
                bool deformitydone = await dodeformity;

                Task<bool> dodwd = DWDSynch();
                bool dwddone = await dodwd;

                Task<bool> dophoto = PhotoSynch();
                bool photodone = await dophoto;
               
                Task<bool> dovegetationcensus = VegetationCensusSynch();
                bool vegetationcensusdone = await dovegetationcensus;


                // finish up by saving the details
                if (projectdone && plotdone && treedone && stemmapdone && ecositedone && soildone && smalltreedone && vegetationdone && deformitydone && dwddone && photodone && persondone && vegetationcensusdone && service.IsSuccess )
                {
                    settings.LastSynched = DateTime.UtcNow;
                    settings.PROJECT_ROWS_SYNCHED = projectpushes;
                    settings.PLOT_ROWS_SYNCHED = plotpushes;
                    settings.TREE_ROWS_SYNCHED = treepushes;
                    settings.PROJECT_ROWS_PULLED = projectpulls;
                    settings.PLOT_ROWS_PULLED = plotpulls;
                    settings.TREE_ROWS_PULLED = treepulls;

                    databasehelper.UpdateSettings(settings);
                    return true;
                }
                else 
                {
                    await Application.Current.MainPage.DisplayAlert("Synch did not finish", service.Msg, "Ok");
                    return false;
                }
            }
            catch (Exception ex)
            { 
                LogWriter logger = new LogWriter();
                logger.LogWrite(ex.Message);
                return false;
            }
        }
        public async Task<bool> RunSynch(String _plotid )
        {
            // DateTime lastsynchdate;
            // this is just a plot level synch process
            if (_plotid == "") return false;
            try
            {
                DateTime basedate = new DateTime(2000, 1, 1, 0, 0, 0);
                DateTime pulldate = new DateTime(2000, 1, 1, 0, 0, 0);

                // Set the last synch date to the plot synch
                settings.LastSynched =  databasehelper.GetPlotLastSynch(_plotid); 
                //confirm the date used for the last synch
                if (DateTime.Compare(settings.LastSynched, basedate) < 0)
                {
                    prevdate = basedate;
                    settings.LastSynched = basedate;
                }

                Task<bool> doperson = PersonSynch();
                bool persondone = await doperson;

                Task<bool> doplot = PlotSynch(_plotid);
                bool plotdone = await doplot;
                string plottype = databasehelper.GetPlotType(_plotid);

                Task<bool> dotree = TreeSynch(_plotid, pulldate, plottype);
                bool treedone = await dotree;

                Task<bool> doecosite = EcositeSynch(_plotid, pulldate);
                bool ecositedone = await doecosite;

                Task<bool> dosoil = SoilSynch(_plotid, pulldate);
                bool soildone = await dosoil;

                Task<bool> dophoto = PhotoSynch();
                bool photodone = await dophoto;

                Task<bool> dosmalltreetally = SmalltreeTallySynch(_plotid, pulldate);
                bool smalltreetallydone = await dosmalltreetally;
                bool smalltreedone = true;

                Task<bool> dosmalltree = SmalltreeSynch(_plotid, pulldate);
                smalltreedone = await dosmalltree;

                bool vegetationdone = true;
                bool dwddone = true;
                bool vegetationcensusdone = true;
                bool deformitydone = true;
                bool stemmapdone = true;
                if (plottype == "B" || plottype == "C")
                {
                  
                    Task<bool> dostemmap = StemmapSynch();
                    stemmapdone = await dostemmap;

                }
                if (plottype == "C")
                {

                    Task<bool> dodeformity = DeformitySynch();
                    deformitydone = await dodeformity;

                    Task<bool> dovegetation = VegetationSynch(_plotid, pulldate);
                    vegetationdone = await dovegetation;

                    Task<bool> dodwd = DWDSynch(_plotid, pulldate);
                    dwddone = await dodwd;

                    Task<bool> dovegetationcensus = VegetationCensusSynch(_plotid, pulldate);
                    vegetationcensusdone = await dovegetationcensus;
                }

                // finish up by saving the details
                if (plotdone && treedone &&  ecositedone && soildone && photodone && persondone && service.IsSuccess && smalltreedone && dwddone && vegetationdone && vegetationcensusdone) {
                  //  settings.LastSynched = DateTime.UtcNow;
                    settings.PROJECT_ROWS_SYNCHED = projectpushes;
                    settings.PLOT_ROWS_SYNCHED = plotpushes;
                    settings.TREE_ROWS_SYNCHED = treepushes;
                    settings.PROJECT_ROWS_PULLED = projectpulls;
                    settings.PLOT_ROWS_PULLED = plotpulls;
                    settings.TREE_ROWS_PULLED = treepulls;

                    databasehelper.UpdateSettings(settings);
                    return true;
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Plot Synch did not finish", service.Msg, "Ok");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogWriter logger = new LogWriter();
                logger.LogWrite(ex.Message);
                return false;
            }
        }

        // run the project table synch
        public async Task<bool> ProjectSynch(bool pullonly = false)
        {
            EntityManager<PROJECT> manager = new EntityManager<PROJECT>(service);
            if (!pullonly)
            {
                var updatelist = databasehelper.GetProjectToUpdate(settings.LastSynched);
                // pull updated records
                Task<List<PROJECT>> getupdatetask = manager.GetTasksAsync(prevdate, null);
                List<PROJECT> updaterecords = await getupdatetask;
                foreach (var itm in updaterecords)
                {
                    if (!updatelist.Any(item => item.PROJECTID == itm.PROJECTID))
                    {
                        databasehelper.UpdateProject(itm);
                    }
                }

                // push new records
                var list = databasehelper.GetProjecttoInsert(settings.LastSynched);
                if (list.Count > 0)
                {
                    Task inserttask = manager.SaveTasksAsync(list, true);
                    await inserttask;
                    projectpushes += 1;
                }
                //push updates

                if (updatelist.Count > 0)
                {
                    Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                    await pushupdatetask;
                }
            }
            // pull new records
            Task<List<PROJECT>> gettask = manager.GetTasksAsync(null, null);
            List<PROJECT> newrecords = await gettask;
            foreach (var itm in newrecords)
            {
                databasehelper.InsertProject(itm);
                projectpulls += 1; 
            }
  
            return true;
        }


        public async Task<bool> PlotSynch(bool pullonly = false)
        {
            EntityManager<PLOT> manager = new EntityManager<PLOT>(service);
            if (!pullonly)
            {

                // get the update list of records
                var updatelist = databasehelper.GetPlotToUpdate(settings.LastSynched);

                // pull updated records
                Task<List<PLOT>> getupdatetask = manager.GetTasksAsync(prevdate, null);
                List<PLOT> updaterecords = await getupdatetask;
                foreach (var itm in updaterecords)
                {
                    // only update records not being updated from the app
                    if (!updatelist.Any(item => item.PLOTID == itm.PLOTID))
                    {
                        databasehelper.UpdatePlot(itm);
                    }
                }

                // push new records
                var list = databasehelper.GetPlottoInsert(settings.LastSynched);
                if (list.Count > 0)
                {
                    Task inserttask = manager.SaveTasksAsync(list, true);
                    await inserttask;
                    plotpushes += 1;
                }
                //push updates
                if (updatelist.Count > 0)
                {
                    Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                    await pushupdatetask;
                }
            }
            // pull new records
            Task<List<PLOT>> gettask = manager.GetTasksAsync(null, null);
            List<PLOT> newrecords = await gettask;
            foreach (var itm in newrecords)
            {
                 databasehelper.InsertPlot(itm);
                 plotpulls += 1;
                
            }

            return true;
        }


        public async Task<bool> PlotSynch(String _plotid)
        {
            EntityManager<PLOT> manager = new EntityManager<PLOT>(service);
            //string filterNew = "?filter=CreatedAtServer gt '" + prevdate + "' AND PLOTID eq '" + _plotid + "'";
            //string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "' AND PLOTID eq '" + _plotid + "'";
            var updatelist = databasehelper.GetPlotToUpdate(settings.LastSynched, _plotid);

            // pull updated records
            Task<List<PLOT>> getupdatetask = manager.GetTasksAsync(prevdate, _plotid);
            List<PLOT> updaterecords = await getupdatetask;

            foreach (var itm in updaterecords)
            {
                bool synchme = false;
                foreach (var thisplot in updatelist)
                {
                    if (thisplot.PLOTID == itm.PLOTID)
                    {
                        if (DateTime.Compare(thisplot.LastSynched, new DateTime(2001, 1, 1, 0, 0, 0)) < 0 && DateTime.Compare(thisplot.LastModified, itm.LastModified) <= 0)
                        {
                            synchme = true;
                        }
                    } 
                }
                // update this plot from the database if conditions are true
                if (!updatelist.Any(item => item.PLOTID == itm.PLOTID) || synchme )  // added this logic to allow plots to synch, that werte brought in with the pull only routine
                    {
                        databasehelper.UpdatePlot(itm);
                    }
            }

            // push new records
            var list = databasehelper.GetPlottoInsert(settings.LastSynched);
            if (list.Count > 0)
            {
                Task inserttask = manager.SaveTasksAsync(list, true);
                await inserttask;
                plotpushes += 1;
            }
            //push updates
           
            if (updatelist.Count > 0)
            {
                Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                await pushupdatetask;
            }

            return true;
        }


        public async Task<bool> TreeSynch()
        {
            EntityManager<TREE> manager = new EntityManager<TREE>(service);
            //string filterNew = "?filter=CreatedAtServer gt '" + prevdate + "'";
            //string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "'";
            var updatelist = databasehelper.GetTreeToUpdate(settings.LastSynched);

            // pull updated records
            Task<List<TREE>> getupdatetask = manager.GetTasksAsync(prevdate, null);
            List<TREE> updaterecords = await getupdatetask;
            foreach (var itm in updaterecords)
            {
                if (!updatelist.Any(item => item.TREEID == itm.TREEID))
                {
                    databasehelper.UpdateTree(itm);
                }
            }

            // push new records
            var list = databasehelper.GetTreetoInsert(settings.LastSynched);
            if (list.Count > 0)
            {
                Task inserttask = manager.SaveTasksAsync(list, true);
                await inserttask;
                treepushes += 1;
            }
            //push updates

            if (updatelist.Count > 0)
            {
                Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                await pushupdatetask;
            }

            if (!util.DoPartialSynch)
            {
                // pull new records
                Task<List<TREE>> gettask = manager.GetTasksAsync(prevdate, null);
                List<TREE> newrecords = await gettask;
                foreach (var itm in newrecords)
                {
                    databasehelper.InsertTree(itm);
                    treepulls += 1;
                }
            }

            //push deletes
            //var deletelist = databasehelper.GetTreeToDelete(settings.LastSynched);
            //foreach (var itm in deletelist)
            //{
            //    Task deletetask = manager.DeleteTaskAsync(itm);
            //    await deletetask;
            //}

            return true;
        }


        public async Task<bool> TreeSynch(String _plotid, DateTime _origdate, String plottype)
        {
            EntityManager<TREE> manager = new EntityManager<TREE>(service);
            string filterNew = "?filter=CreatedAtServer gt '" + _origdate + "' AND PLOTID eq '" + _plotid + "'";
            string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "' AND PLOTID eq '" + _plotid + "'";
            var updatelist = databasehelper.GetTreeToUpdate(settings.LastSynched, _plotid);
            // pull updated records
            Task<List<TREE>> getupdatetask = manager.GetTasksAsync(prevdate, _plotid);
            List<TREE> updaterecords = await getupdatetask;
            foreach (var itm in updaterecords)
            {
                if (!updatelist.Any(item => item.TREEID == itm.TREEID))
                {
                    databasehelper.UpdateTree(itm);
                }
            }

            // push new records
            var list = databasehelper.GetTreetoInsert(settings.LastSynched);
            if (list.Count > 0)
            {
                Task inserttask = manager.SaveTasksAsync(list, true);
                await inserttask;
                treepushes += 1;
            }
            //push updates

            if (updatelist.Count > 0)
            {
                Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                await pushupdatetask;
            }

            // pull new records
            Task<List<TREE>> gettask = manager.GetTasksAsync(null, _plotid);
            List<TREE> newrecords = await gettask;
            foreach (var itm in newrecords)
            {
                databasehelper.InsertTree(itm);
                //                filterTree = filterTree + "'" + itm.TREEID + "',";
                treepulls += 1;
            }
            // now use the trees to makes a list for synching

            return true;
        }

        public async Task<bool> StemmapSynch()
        {
            EntityManager<STEMMAP> manager = new EntityManager<STEMMAP>(service);
            string filterNew = "?filter=CreatedAtServer gt '" + prevdate + "'";
            string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "'";
            var updatelist = databasehelper.GetStemmapToUpdate(settings.LastSynched);
            // pull updated records
            Task<List<STEMMAP>> getupdatetask = manager.GetTasksAsync(prevdate, null);
            List<STEMMAP> updaterecords = await getupdatetask;
            foreach (var itm in updaterecords)
            {
                if (!updatelist.Any(item => item.STEMMAPID == itm.STEMMAPID))
                {
                    databasehelper.UpdateStemmap(itm);
                }
            }

            // push new records
            var list = databasehelper.GetStemmaptoInsert(settings.LastSynched);
            if (list.Count > 0)
            {
                Task inserttask = manager.SaveTasksAsync(list, true);
                await inserttask;

            }
            //push updates

            if (updatelist.Count > 0)
            {
                Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                await pushupdatetask;
            }
            if (!util.DoPartialSynch)
            {
                // pull new records
                Task<List<STEMMAP>> gettask = manager.GetTasksAsync(null, null);
                List<STEMMAP> newrecords = await gettask;
                foreach (var itm in newrecords)
                {
                    databasehelper.InsertStemmap(itm);
                }
            }


            return true;
        }

        //public async Task<bool> StemmapSynch(String _origdate, string infilter)
        //{
        //    EntityManager<STEMMAP> manager = new EntityManager<STEMMAP>(service);

        //    string filterNew = "?filter=CreatedAtServer gt '" + _origdate + "' AND (" + infilter + ")";
        //    string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "' AND (" + infilter + ")";

        //    // pull updated records
        //    Task<List<STEMMAP>> getupdatetask = manager.GetTasksAsync(null, prevdate, null);
        //    List<STEMMAP> updaterecords = await getupdatetask;
        //    foreach (var itm in updaterecords)
        //    {
        //        databasehelper.UpdateStemmap(itm);
        //    }

        //    // pull new records
        //    Task<List<STEMMAP>> gettask = manager.GetTasksAsync(_origdate, null, null);
        //    List<STEMMAP> newrecords = await gettask;
        //    foreach (var itm in newrecords)
        //    {
        //        databasehelper.InsertStemmap(itm);
        //    }
        //    return true;
        //}
        public async Task<bool> EcositeSynch()
        {
            EntityManager<ECOSITE> manager = new EntityManager<ECOSITE>(service);
            string filterNew = "?filter=CreatedAtServer gt '" + prevdate + "'";
            string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "'";
            var updatelist = databasehelper.GetEcositeToUpdate(settings.LastSynched);
            // pull updated records
            Task<List<ECOSITE>> getupdatetask = manager.GetTasksAsync(prevdate, null);
            List<ECOSITE> updaterecords = await getupdatetask;
            foreach (var itm in updaterecords)
            {
                if (!updatelist.Any(item => item.ECOSITEID == itm.ECOSITEID))
                {
                    databasehelper.UpdateEcosite(itm);
                }
            }

            // push new records
            var list = databasehelper.GetEcositetoInsert(settings.LastSynched);
            if (list.Count > 0)
            {
                Task inserttask = manager.SaveTasksAsync(list, true);
                await inserttask;

            }
            //push updates

            if (updatelist.Count > 0)
            {
                Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                await pushupdatetask;
            }
            if (!util.DoPartialSynch)
            {
                // pull new records
                Task<List<ECOSITE>> gettask = manager.GetTasksAsync(null, null);
                List<ECOSITE> newrecords = await gettask;
                foreach (var itm in newrecords)
                {
                    databasehelper.InsertEcosite(itm);
                }
            }

            return true;
        }


        public async Task<bool> EcositeSynch(String _plotid, DateTime _origdate)
        {
            EntityManager<ECOSITE> manager = new EntityManager<ECOSITE>(service);
            //string filterNew = "?filter=CreatedAtServer gt '" + _origdate + "' AND PLOTID eq '" + _plotid + "'";
            //string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "' AND PLOTID eq '" + _plotid + "'";
            var updatelist = databasehelper.GetEcositeToUpdate(settings.LastSynched, _plotid);

            // pull updated records
            Task<List<ECOSITE>> getupdatetask = manager.GetTasksAsync(prevdate, _plotid);
            List<ECOSITE> updaterecords = await getupdatetask;
            foreach (var itm in updaterecords)
            {

                if (!updatelist.Any(item => item.ECOSITEID == itm.ECOSITEID))
                {
                    databasehelper.UpdateEcosite(itm);
                }
            }

            // push new records
            var list = databasehelper.GetEcositetoInsert(settings.LastSynched);
            if (list.Count > 0)
            {
                Task inserttask = manager.SaveTasksAsync(list, true);
                await inserttask;

            }
            //push updates

            if (updatelist.Count > 0)
            {
                Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                await pushupdatetask;
            }
            // pull new records
            Task<List<ECOSITE>> gettask = manager.GetTasksAsync(null, _plotid);
            List<ECOSITE> newrecords = await gettask;
            foreach (var itm in newrecords)
            {
                databasehelper.InsertEcosite(itm);
            }
            return true;
        }

        public async Task<bool> SoilSynch()
        {
            EntityManager<SOIL> manager = new EntityManager<SOIL>(service);
            //string filterNew = "?filter=CreatedAtServer gt '" + prevdate + "'";
            //string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "'";
            var updatelist = databasehelper.GetSoilToUpdate(settings.LastSynched);
            // pull updated records
            Task<List<SOIL>> getupdatetask = manager.GetTasksAsync(prevdate, null);
            List<SOIL> updaterecords = await getupdatetask;
            foreach (var itm in updaterecords)
            {
                if (!updatelist.Any(item => item.SOILID == itm.SOILID))
                {
                    databasehelper.UpdateSoil(itm);
                }
            }

            // push new records
            var list = databasehelper.GetSoiltoInsert(settings.LastSynched);
            if (list.Count > 0)
            {
                Task inserttask = manager.SaveTasksAsync(list, true);
                await inserttask;

            }
            //push updates

            if (updatelist.Count > 0)
            {
                Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                await pushupdatetask;
            }
            if (!util.DoPartialSynch)
            {
                // pull new records
                Task<List<SOIL>> gettask = manager.GetTasksAsync(null, null);
                List<SOIL> newrecords = await gettask;
                foreach (var itm in newrecords)
                {
                    databasehelper.InsertSoil(itm);
                }
            }

            //push deletes
            //var deletelist = databasehelper.GetSoilToDelete(settings.LastSynched);
            //foreach (var itm in deletelist)
            //{
            //    Task deletetask = manager.DeleteTaskAsync(itm);
            //    await deletetask;
            //}
            return true;
        }

        public async Task<bool> SoilSynch(String _plotid, DateTime _origdate)
        {
            EntityManager<SOIL> manager = new EntityManager<SOIL>(service);
            //string filterNew = "?filter=CreatedAtServer gt '" + _origdate + "' AND PLOTID eq '" + _plotid + "'";
            //string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "' AND PLOTID eq '" + _plotid + "'";
            var updatelist = databasehelper.GetSoilToUpdate(settings.LastSynched, _plotid);
            // pull updated records
            Task<List<SOIL>> getupdatetask = manager.GetTasksAsync(prevdate, _plotid);
            List<SOIL> updaterecords = await getupdatetask;
            foreach (var itm in updaterecords)
            {
                if (!updatelist.Any(item => item.SOILID == itm.SOILID))
                {
                    databasehelper.UpdateSoil(itm);
                }
            }

            // push new records
            var list = databasehelper.GetSoiltoInsert(settings.LastSynched);
            if (list.Count > 0)
            {
                Task inserttask = manager.SaveTasksAsync(list, true);
                await inserttask;

            }
            //push updates

            if (updatelist.Count > 0)
            {
                Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                await pushupdatetask;
            }
            // pull new records
            Task<List<SOIL>> gettask = manager.GetTasksAsync(null, _plotid);
            List<SOIL> newrecords = await gettask;
            foreach (var itm in newrecords)
            {
                databasehelper.InsertSoil(itm);
            }
            return true;
        }

        public async Task<bool> SmalltreeSynch()
        {
            EntityManager<SMALLTREE> manager = new EntityManager<SMALLTREE>(service);
            //string filterNew = "?filter=CreatedAtServer gt '" + prevdate + "'";
            //string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "'";
            var updatelist = databasehelper.GetSmalltreeToUpdate(settings.LastSynched);
            // pull updated records
            Task<List<SMALLTREE>> getupdatetask = manager.GetTasksAsync(prevdate, null);
            List<SMALLTREE> updaterecords = await getupdatetask;
            foreach (var itm in updaterecords)
            {
                if (!updatelist.Any(item => item.SMALLTREEID == itm.SMALLTREEID))
                {
                    databasehelper.UpdateSmallTree(itm);
                }
            }

            // push new records
            var list = databasehelper.GetSmalltreetoInsert(settings.LastSynched);
            if (list.Count > 0)
            {
                Task inserttask = manager.SaveTasksAsync(list, true);
                await inserttask;

            }
            //push updates

            if (updatelist.Count > 0)
            {
                Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                await pushupdatetask;
            }
            if (!util.DoPartialSynch)
            {
                // pull new records
                Task<List<SMALLTREE>> gettask = manager.GetTasksAsync(null, null);
                List<SMALLTREE> newrecords = await gettask;
                foreach (var itm in newrecords)
                {
                    databasehelper.InsertSmallTree(itm);
                }
            }
            return true;
        }



        public async Task<bool> SmalltreeSynch(String _plotid, DateTime _origdate)
        {
            EntityManager<SMALLTREE> manager = new EntityManager<SMALLTREE>(service);
            //string filterNew = "?filter=CreatedAtServer gt '" + _origdate + "' AND PLOTID eq '" + _plotid + "'";
            //string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "' AND PLOTID eq '" + _plotid + "'";
            var updatelist = databasehelper.GetSmalltreeToUpdate(settings.LastSynched, _plotid);
            // pull updated records
            Task<List<SMALLTREE>> getupdatetask = manager.GetTasksAsync(prevdate, _plotid);
            List<SMALLTREE> updaterecords = await getupdatetask;
            foreach (var itm in updaterecords)
            {
                if (!updatelist.Any(item => item.SMALLTREEID == itm.SMALLTREEID))
                {
                    databasehelper.UpdateSmallTree(itm);
                }
            }

            // push new records
            var list = databasehelper.GetSmalltreetoInsert(settings.LastSynched);
            if (list.Count > 0)
            {
                Task inserttask = manager.SaveTasksAsync(list, true);
                await inserttask;

            }
            //push updates

            if (updatelist.Count > 0)
            {
                Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                await pushupdatetask;
            }

            // pull new records
            Task<List<SMALLTREE>> gettask = manager.GetTasksAsync(null, _plotid);
            List<SMALLTREE> newrecords = await gettask;
            foreach (var itm in newrecords)
            {
                databasehelper.InsertSmallTree(itm);
            }

            return true;
        }


        public async Task<bool> SmalltreeTallySynch()
        {
            EntityManager<SMALLTREETALLY> manager = new EntityManager<SMALLTREETALLY>(service);
            //string filterNew = "?filter=CreatedAtServer gt '" + prevdate + "'";
            //string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "'";
            var updatelist = databasehelper.GetSmalltreeTallyToUpdate(settings.LastSynched);
            // pull updated records
            Task<List<SMALLTREETALLY>> getupdatetask = manager.GetTasksAsync(prevdate, null);
            List<SMALLTREETALLY> updaterecords = await getupdatetask;
            foreach (var itm in updaterecords)
            {
                if (!updatelist.Any(item => item.SMALLTREETALLYID == itm.SMALLTREETALLYID))
                {
                    databasehelper.UpdateSmallTreeTally(itm);
                }
            }

            // push new records
            var list = databasehelper.GetSmalltreeTallytoInsert(settings.LastSynched);
            if (list.Count > 0)
            {
                Task inserttask = manager.SaveTasksAsync(list, true);
                await inserttask;

            }
            //push updates
            if (updatelist.Count > 0)
            {
                Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                await pushupdatetask;
            }
            if (!util.DoPartialSynch)
            {
                // pull new records
                Task<List<SMALLTREETALLY>> gettask = manager.GetTasksAsync(null, null);
                List<SMALLTREETALLY> newrecords = await gettask;
                foreach (var itm in newrecords)
                {
                    databasehelper.InsertSmallTreeTally(itm);
                }
            }
            return true;
        }


        public async Task<bool> SmalltreeTallySynch(String _plotid, DateTime _origdate)
        {
            EntityManager<SMALLTREETALLY> manager = new EntityManager<SMALLTREETALLY>(service);
            //string filterNew = "?filter=CreatedAtServer gt '" + _origdate + "' AND PLOTID eq '" + _plotid + "'";
            //string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "' AND PLOTID eq '" + _plotid + "'";
            var updatelist = databasehelper.GetSmalltreeTallyToUpdate(settings.LastSynched, _plotid);
            // pull updated records
            Task<List<SMALLTREETALLY>> getupdatetask = manager.GetTasksAsync(prevdate, _plotid);
            List<SMALLTREETALLY> updaterecords = await getupdatetask;
            foreach (var itm in updaterecords)
            {
                if (!updatelist.Any(item => item.SMALLTREETALLYID == itm.SMALLTREETALLYID))
                {
                    databasehelper.UpdateSmallTreeTally(itm);
                }
            }

            // push new records
            var list = databasehelper.GetSmalltreeTallytoInsert(settings.LastSynched);
            if (list.Count > 0)
            {
                Task inserttask = manager.SaveTasksAsync(list, true);
                await inserttask;

            }
            //push updates

            if (updatelist.Count > 0)
            {
                Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                await pushupdatetask;
            }

            // pull new records
            Task<List<SMALLTREETALLY>> gettask = manager.GetTasksAsync(null, _plotid);
            List<SMALLTREETALLY> newrecords = await gettask;
            foreach (var itm in newrecords)
            {
                databasehelper.InsertSmallTreeTally(itm);
            }

            return true;
        }

        public async Task<bool> VegetationSynch()
        {
            EntityManager<VEGETATION> manager = new EntityManager<VEGETATION>(service);
            //string filterNew = "?filter=CreatedAtServer gt '" + prevdate + "'";
            //string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "'";
            var updatelist = databasehelper.GetVegetationToUpdate(settings.LastSynched);
            // pull updated records
            Task<List<VEGETATION>> getupdatetask = manager.GetTasksAsync(prevdate, null);
            List<VEGETATION> updaterecords = await getupdatetask;
            foreach (var itm in updaterecords)
            {
                if (!updatelist.Any(item => item.VEGETATIONID == itm.VEGETATIONID))
                {
                    databasehelper.UpdateVegetation(itm);
                }
            }

            // push new records
            var list = databasehelper.GetVegetationtoInsert(settings.LastSynched);
            if (list.Count > 0)
            {
                Task inserttask = manager.SaveTasksAsync(list, true);
                await inserttask;

            }
            //push updates

            if (updatelist.Count > 0)
            {
                Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                await pushupdatetask;
            }
            if (!util.DoPartialSynch)
            {
                // pull new records
                Task<List<VEGETATION>> gettask = manager.GetTasksAsync(null, null);
                List<VEGETATION> newrecords = await gettask;
                foreach (var itm in newrecords)
                {
                    databasehelper.InsertVegetation(itm);
                }
            }


            return true;
        }


        public async Task<bool> VegetationSynch(String _plotid, DateTime _origdate)
        {
            EntityManager<VEGETATION> manager = new EntityManager<VEGETATION>(service);
            //string filterNew = "?filter=CreatedAtServer gt '" + _origdate + "' AND PLOTID eq '" + _plotid + "'";
            //string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "' AND PLOTID eq '" + _plotid + "'";
            var updatelist = databasehelper.GetVegetationToUpdate(settings.LastSynched, _plotid);
            // pull updated records
            Task<List<VEGETATION>> getupdatetask = manager.GetTasksAsync(prevdate, _plotid);
            List<VEGETATION> updaterecords = await getupdatetask;
            foreach (var itm in updaterecords)
            {
                if (!updatelist.Any(item => item.VEGETATIONID == itm.VEGETATIONID))
                {
                    databasehelper.UpdateVegetation(itm);
                }
            }

            // push new records
            var list = databasehelper.GetVegetationtoInsert(settings.LastSynched);
            if (list.Count > 0)
            {
                Task inserttask = manager.SaveTasksAsync(list, true);
                await inserttask;

            }
            //push updates

            if (updatelist.Count > 0)
            {
                Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                await pushupdatetask;
            }

            // pull new records
            Task<List<VEGETATION>> gettask = manager.GetTasksAsync(null, _plotid);
            List<VEGETATION> newrecords = await gettask;
            foreach (var itm in newrecords)
            {
                databasehelper.InsertVegetation(itm);
            }

            return true;
        }
        public async Task<bool> DeformitySynch()
        {
            var manager = new EntityManager<DEFORMITY>(service);
            //string filterNew = "?filter=CreatedAtServer gt '" + prevdate + "'";
            //string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "'";
            var updatelist = databasehelper.GetDeformityToUpdate(settings.LastSynched);
            // pull updated records
            Task<List<DEFORMITY>> getupdatetask = manager.GetTasksAsync(prevdate, null);
            List<DEFORMITY> updaterecords = await getupdatetask;
            foreach (var itm in updaterecords)
            {
                if (!updatelist.Any(item => item.DEFORMITYID == itm.DEFORMITYID))
                {
                    databasehelper.UpdateDeformity(itm);
                }
            }

            // push new records
            var list = databasehelper.GetDeformitytoInsert(settings.LastSynched);
            if (list.Count > 0)
            {
                Task inserttask = manager.SaveTasksAsync(list, true);
                await inserttask;

            }
            //push updates

            if (updatelist.Count > 0)
            {
                Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                await pushupdatetask;
            }
            if (!util.DoPartialSynch)
            {
                // pull new records
                Task<List<DEFORMITY>> gettask = manager.GetTasksAsync(null, null);
                List<DEFORMITY> newrecords = await gettask;
                foreach (var itm in newrecords)
                {
                    databasehelper.InsertDeformity(itm);
                }
            }
            return true;
        }


        //public async Task<bool> DeformitySynch(String _origdate, string infilter)
        //{

        //    string filterNew = "?filter=CreatedAtServer gt '" + _origdate + "' AND (" + infilter + ")";
        //    string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "' AND (" + infilter + ")";

        //    EntityManager<DEFORMITY> manager = new EntityManager<DEFORMITY>(service);

        //    // pull updated records
        //    Task<List<DEFORMITY>> getupdatetask = manager.GetTasksAsync(filterUpdate);
        //    List<DEFORMITY> updaterecords = await getupdatetask;
        //    foreach (var itm in updaterecords)
        //    {
        //        databasehelper.UpdateDeformity(itm);
        //    }

        //    // pull new records
        //    Task<List<DEFORMITY>> gettask = manager.GetTasksAsync(filterNew);
        //    List<DEFORMITY> newrecords = await gettask;
        //    foreach (var itm in newrecords)
        //    {
        //        databasehelper.InsertDeformity(itm);
        //    }

        //    return true;
        //}

        public async Task<bool> DWDSynch()
        {
            EntityManager<DWD> manager = new EntityManager<DWD>(service);
            //string filterNew = "?filter=CreatedAtServer gt '" + prevdate + "'";
            //string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "'";
            var updatelist = databasehelper.GetDWDToUpdate(settings.LastSynched);
            // pull updated records
            Task<List<DWD>> getupdatetask = manager.GetTasksAsync(prevdate, null);
            List<DWD> updaterecords = await getupdatetask;
            foreach (var itm in updaterecords)
            {
                if (!updatelist.Any(item => item.DWDID == itm.DWDID))
                {
                    databasehelper.UpdateDWD(itm);
                }
            }

            // push new records
            var list = databasehelper.GetDWDtoInsert(settings.LastSynched);
            if (list.Count > 0)
            {
                Task inserttask = manager.SaveTasksAsync(list, true);
                await inserttask;

            }
            //push updates

            if (updatelist.Count > 0)
            {
                Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                await pushupdatetask;
            }
            if (!util.DoPartialSynch)
            {
                // pull new records
                Task<List<DWD>> gettask = manager.GetTasksAsync(null, null);
                List<DWD> newrecords = await gettask;
                foreach (var itm in newrecords)
                {
                    databasehelper.InsertDWD(itm);
                }
            }


            return true;
        }


        public async Task<bool> DWDSynch(String _plotid, DateTime _origdate)
        {
            EntityManager<DWD> manager = new EntityManager<DWD>(service);
            //string filterNew = "?filter=CreatedAtServer gt '" + _origdate + "' AND PLOTID eq '" + _plotid + "'";
            //string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "' AND PLOTID eq '" + _plotid + "'";
            var updatelist = databasehelper.GetDWDToUpdate(settings.LastSynched, _plotid);
            // pull updated records
            Task<List<DWD>> getupdatetask = manager.GetTasksAsync(prevdate, _plotid);
            List<DWD> updaterecords = await getupdatetask;
            foreach (var itm in updaterecords)
            {
                if (!updatelist.Any(item => item.DWDID == itm.DWDID))
                {
                    databasehelper.UpdateDWD(itm);
                }
            }

            // push new records
            var list = databasehelper.GetDWDtoInsert(settings.LastSynched);
            if (list.Count > 0)
            {
                Task inserttask = manager.SaveTasksAsync(list, true);
                await inserttask;

            }
            //push updates

            if (updatelist.Count > 0)
            {
                Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                await pushupdatetask;
            }

            // pull new records
            Task<List<DWD>> gettask = manager.GetTasksAsync(null, _plotid);
            List<DWD> newrecords = await gettask;
            foreach (var itm in newrecords)
            {
                databasehelper.InsertDWD(itm);
            }

            return true;
        }


        public async Task<bool> PhotoSynch()
        {
            EntityManager<PHOTO> manager = new EntityManager<PHOTO>(service);
            //string filterNew = "?filter=CreatedAtServer gt '" + prevdate + "'";
            //string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "'";
            var updatelist = databasehelper.GetPhotoToUpdate(settings.LastSynched);
            // pull updated records
            Task<List<PHOTO>> getupdatetask = manager.GetTasksAsync(prevdate, null);
            List<PHOTO> updaterecords = await getupdatetask;
            foreach (var itm in updaterecords)
            {
                if (!updatelist.Any(item => item.PHOTOID == itm.PHOTOID))
                {
                    databasehelper.UpdatePhoto(itm);
                }
            }

            // push new records
            var list = databasehelper.GetPhototoInsert(settings.LastSynched);
            if (list.Count > 0)
            {
                Task inserttask = manager.SaveTasksAsync(list, true);
                await inserttask;

            }
            //push updates

            if (updatelist.Count > 0)
            {
                Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                await pushupdatetask;
            }

            // pull new records
            Task<List<PHOTO>> gettask = manager.GetTasksAsync(null, null);
            List<PHOTO> newrecords = await gettask;
            foreach (var itm in newrecords)
            {
                databasehelper.InsertPhoto(itm);
            }


            return true;
        }


        public async Task<bool> PhotoSynch(String _plotid, DateTime _origdate)
        {
            EntityManager<PHOTO> manager = new EntityManager<PHOTO>(service);
            //string filterNew = "?filter=CreatedAtServer gt '" + _origdate + "' AND PLOTID eq '" + _plotid + "'";
            //string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "' AND PLOTID eq '" + _plotid + "'";
            var updatelist = databasehelper.GetPhotoToUpdate(settings.LastSynched, _plotid);
            // pull updated records
            Task<List<PHOTO>> getupdatetask = manager.GetTasksAsync(prevdate, _plotid);
            List<PHOTO> updaterecords = await getupdatetask;
            foreach (var itm in updaterecords)
            {
                if (!updatelist.Any(item => item.PHOTOID == itm.PHOTOID))
                {
                    databasehelper.UpdatePhoto(itm);
                }
            }

            // push new records
            var list = databasehelper.GetPhototoInsert(settings.LastSynched);
            if (list.Count > 0)
            {
                Task inserttask = manager.SaveTasksAsync(list, true);
                await inserttask;

            }
            //push updates

            if (updatelist.Count > 0)
            {
                Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                await pushupdatetask;
            }

            // pull new records
            Task<List<PHOTO>> gettask = manager.GetTasksAsync(null, _plotid);
            List<PHOTO> newrecords = await gettask;
            foreach (var itm in newrecords)
            {
                databasehelper.InsertPhoto(itm);
            }

            return true;
        }
        public async Task<bool> PersonSynch(bool pullonly = false)
        {
            EntityManager<PERSON> manager = new EntityManager<PERSON>(service);
            //string filterNew = "?filter=CreatedAtServer gt '01-01-2020'";
            //string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "'";
            if (!pullonly)
            {

                var updatelist = databasehelper.GetPersonToUpdate(settings.LastSynched);
                // pull updated records
                Task<List<PERSON>> getupdatetask = manager.GetTasksAsync(prevdate, null);
                List<PERSON> updaterecords = await getupdatetask;
                foreach (var itm in updaterecords)
                {
                    if (!updatelist.Any(item => item.PERSONID == itm.PERSONID))
                    {
                        databasehelper.UpdatePerson(itm);
                    }
                }

                // push new records
                var list = databasehelper.GetPersontoInsert(settings.LastSynched);
                if (list.Count > 0)
                {
                    Task inserttask = manager.SaveTasksAsync(list, true);
                    await inserttask;

                }
                //push updates

                if (updatelist.Count > 0)
                {
                    Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                    await pushupdatetask;
                }
            }
            // pull new records
            Task<List<PERSON>> gettask = manager.GetTasksAsync(null, null);
            List<PERSON> newrecords = await gettask;
            foreach (var itm in newrecords)
            {
                databasehelper.InsertPerson(itm);
            }


            return true;
        }
        public async Task<bool> VegetationCensusSynch()
        {
            EntityManager<VEGETATIONCENSUS> manager = new EntityManager<VEGETATIONCENSUS>(service);
            //string filterNew = "?filter=CreatedAtServer gt '" + prevdate + "'";
            //string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "'";
            var updatelist = databasehelper.GetVegetationCensusToUpdate(settings.LastSynched);
            // pull updated records
            Task<List<VEGETATIONCENSUS>> getupdatetask = manager.GetTasksAsync(prevdate, null);
            List<VEGETATIONCENSUS> updaterecords = await getupdatetask;
            foreach (var itm in updaterecords)
            {
                if (!updatelist.Any(item => item.VEGETATIONCENSUSID == itm.VEGETATIONCENSUSID))
                {
                    databasehelper.UpdateVegetation(itm);
                }
            }

            // push new records
            var list = databasehelper.GetVegetationCensustoInsert(settings.LastSynched);
            if (list.Count > 0)
            {
                Task inserttask = manager.SaveTasksAsync(list, true);
                await inserttask;

            }
            //push updates

            if (updatelist.Count > 0)
            {
                Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                await pushupdatetask;
            }
            if (!util.DoPartialSynch)
            {
                // pull new records
                Task<List<VEGETATIONCENSUS>> gettask = manager.GetTasksAsync(null, null);
                List<VEGETATIONCENSUS> newrecords = await gettask;
                foreach (var itm in newrecords)
                {
                    databasehelper.InsertVegetationCensus(itm);
                }
            }

            return true;
        }
        public async Task<bool> VegetationCensusSynch(String _plotid, DateTime _origdate)
        {
            EntityManager<VEGETATIONCENSUS> manager = new EntityManager<VEGETATIONCENSUS>(service);
            //string filterNew = "?filter=CreatedAtServer gt '" + _origdate + "' AND PLOTID eq '" + _plotid + "'";
            //string filterUpdate = "?filter=LastModifiedAtServer gt '" + prevdate + "' AND PLOTID eq '" + _plotid + "'";
            var updatelist = databasehelper.GetVegetationCensusToUpdate(settings.LastSynched, _plotid);
            // pull updated records
            Task<List<VEGETATIONCENSUS>> getupdatetask = manager.GetTasksAsync(prevdate, _plotid);
            List<VEGETATIONCENSUS> updaterecords = await getupdatetask;
            foreach (var itm in updaterecords)
            {
                if (!updatelist.Any(item => item.VEGETATIONCENSUSID == itm.VEGETATIONCENSUSID))
                {
                    databasehelper.UpdateVegetation(itm);
                }
            }

            // push new records
            var list = databasehelper.GetVegetationCensustoInsert(settings.LastSynched);
            if (list.Count > 0)
            {
                Task inserttask = manager.SaveTasksAsync(list, true);
                await inserttask;

            }
            //push updates

            if (updatelist.Count > 0)
            {
                Task pushupdatetask = manager.SaveTasksAsync(updatelist, false);
                await pushupdatetask;
            }

            // pull new records
            Task<List<VEGETATIONCENSUS>> gettask = manager.GetTasksAsync(null, _plotid);
            List<VEGETATIONCENSUS> newrecords = await gettask;
            foreach (var itm in newrecords)
            {
                databasehelper.InsertVegetationCensus(itm);
            }
            return true;
        }

        DateTime GetDate()
        {
            DateTime initdate = settings.LastSynched;
            prevsynchdate = initdate;
            return prevsynchdate; 
        }
    }
}
