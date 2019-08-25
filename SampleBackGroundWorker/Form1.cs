using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SampleBackGroundWorker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            this.Load += Form1_Load;
            this.btnStart.Click += Click_btnStart;
            this.btnCancel.Click += Click_btnCancel;
            this.btnError.Click += Click_btnError;

            // BackgroundWorker 
            this.bgWorker.DoWork += bgWorker_DoWork;
            this.bgWorker.ProgressChanged += bgWorker_ProgressChanged;
            this.bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // 작업 취소를 사용
                this.bgWorker.WorkerSupportsCancellation = true;

                // 작업 진행률 업데이트 보고를 사용
                this.bgWorker.WorkerReportsProgress = true;
                this.btnStart.Enabled = true;
                this.btnCancel.Enabled = false;

                this.lblWorkCount.Text = "0";
            }
            catch (Exception ex)
            {
                throw new Exception("오류가 발생했습니다", ex);
            }
        }

        #region 메서드

        /// <summary>
        /// 처리할 작업입니다.
        /// </summary>
        private void DoWork(BackgroundWorker worker, DoWorkEventArgs e)
        {
            /*
             * 실행중인 Form 과 다른 쓰레드에서 동작하므로 
             * 처리할 메서드에서는 UI 객체의 속성값(Value, Text 등..)을 사용하지 못합니다.
             * 
             * 작업에 필요한 값은 매개변수로 전달받아야 하고 UI객체의 상태를 변화시킬 필요가 있는 경우
             * ProgressChanged
             * RunWorkerCompleted
             * 이벤트를 사용해야 합니다.
             */

            double nMax = 100.0;
            int nExe = 0;

            while (nMax > nExe)
            {
                // 작업이 취소 요청이 되었는지 검사
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    if (ErrorFlag)
                    {
                        ErrorFlag = false;
                        throw new Exception("예외 발생 시뮬레이션");
                    }

                    // 시간이 걸리는 작업을 실행합니다.
                    System.Threading.Thread.Sleep(200);
                    nExe++;

                    // 진행률 업데이트하기 위해 ProgressChanged 이벤트를 발생시킵니다.
                    worker.ReportProgress((int)((nExe / nMax) * 100));
                }
            }
        }

        private void AppendMessage(string message)
        {
            textBox1.AppendText($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]: {message}{Environment.NewLine}");
            textBox1.ScrollToCaret();
        }

        #endregion

        #region 이벤트 핸들러

        /// <summary>
        /// 시작버튼 클릭
        /// </summary>
        private void Click_btnStart(object sender, EventArgs e)
        {
            try
            {
                this.progressBar.Value = 0;
                this.progressBar.Maximum = 100;
                this.progressBar.Step = 1;

                this.lblWorkCount.Text = "0";

                this.btnStart.Enabled = false;
                this.btnCancel.Enabled = true;

                this.Cursor = Cursors.WaitCursor;

                AppendMessage("작업을 시작합니다.");

                // 비동기로 작업을 시작합니다. DoWork 이벤트를 발생
                this.bgWorker.RunWorkerAsync(null);
            }
            catch (Exception ex)
            {
                throw new Exception("오류가 발생했습니다", ex);
            }
        }

        /// <summary>
        /// 취소 버튼 클릭
        /// </summary>
        private void Click_btnCancel(object sender, EventArgs e)
        {
            try
            {
                // 작업 취소요청
                this.bgWorker.CancelAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("오류가 발생했습니다", ex);
            }
        }

        /// <summary>
        /// 예외 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Click_btnError(object sender, EventArgs e)
        {
            ErrorFlag = true;
        }

        /// <summary>
        /// 작업을 시작
        /// </summary>
        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.DoWork((BackgroundWorker)sender, e);
            e.Result = null;

            //try
            //{
            //    this.DoWork((BackgroundWorker)sender, e);
            //    e.Result = null;
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception("오류가 발생했습니다", ex);
            //}
        }

        /// <summary>
        /// 작업 진행률을 업데이트
        /// </summary>
        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                this.progressBar.Value = e.ProgressPercentage;
                int nTmp = 0;
                int.TryParse(this.lblWorkCount.Text, out nTmp);
                nTmp++;
                this.lblWorkCount.Text = nTmp.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("오류가 발생했습니다", ex);
            }
        }

        /// <summary>
        /// 작업이 완료상태(예외, 취소, 완료)가 되면 발생하는 이벤트입니다
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)    // 예외 발생
                {
                    AppendMessage(String.Format("{0} ==> {1}", "예외가 발생했습니다.", e.Error.Message));
                }
                else if (e.Cancelled)   // 작업취소
                {
                    AppendMessage("작업이 취소되었습니다.");
                }
                else                    // 완료
                {
                    AppendMessage("작업이 완료되었습니다.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("오류가 발생했습니다", ex);
            }
            finally
            {
                this.btnStart.Enabled = true;
                this.btnCancel.Enabled = false;
                this.Cursor = Cursors.Default;
            }
        }

        #endregion

        private static bool ErrorFlag = false;
    }
}
