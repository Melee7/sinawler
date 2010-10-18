using System;
using System.Collections.Generic;
using System.Text;
using Sina.Api;
using System.Threading;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Sinawler.Model;
using System.Data;

namespace Sinawler
{
    class StatusRobot:RobotBase
    {
        private long lCommentUID = 0;    //�����˵�UID
        private LinkedList<long> lstRetweetedStatus = new LinkedList<long>();   //ת��΢��ID

        //���캯������Ҫ������Ӧ������΢��API��������
        public StatusRobot ( SinaApiService oAPI ):base(oAPI)
        {
            queueBuffer = new QueueBuffer( QueueBufferTarget.FOR_STATUS );
            strLogFile = Application.StartupPath + "\\" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + "_status.log";
        }

        /// <summary>
        /// ��ʼ����΢��
        /// </summary>
        public void Start ()
        {   
            while (lstWaitingUID.Count == 0) Thread.Sleep(1);   //������Ϊ�գ���ȴ�
            long lStartUID = lstWaitingUID.First.Value;
            while(true)
            {
                if (blnAsyncCancelled) return;
                while (blnSuspending)
                {
                    if (blnAsyncCancelled) return;
                    Thread.Sleep( 50 );
                }
                //����ͷȡ��
                lCurrentUID = lstWaitingUID.First.Value;
                lstWaitingUID.RemoveFirst();
                //�����ݿ���л���������Ԫ��
                long lHead = queueBuffer.Dequeue();
                if (lHead > 0)
                    lstWaitingUID.AddLast( lHead );
                #region Ԥ����
                if (lCurrentUID == lStartUID)  //˵������һ��ѭ������
                {
                    if (blnAsyncCancelled) return;
                    while (blnSuspending)
                    {
                        if (blnAsyncCancelled) return;
                        Thread.Sleep( 50 );
                    }

                    //��־
                    strLog = DateTime.Now.ToString() + "  " + "��ʼ����֮ǰ���ӵ�������...";
                    bwAsync.ReportProgress( 100 );
                    Thread.Sleep( 5 );

                    Status.NewIterate();
                    Comment.NewIterate();
                }

                #endregion                
                #region �û�΢����Ϣ
                if (blnAsyncCancelled) return;
                while (blnSuspending)
                {
                    if (blnAsyncCancelled) return;
                    Thread.Sleep( 50 );
                }
                //��־
                strLog = DateTime.Now.ToString() + "  " + "��ȡ���ݿ����û�" + lCurrentUID.ToString() + "����һ��΢����ID...";
                bwAsync.ReportProgress( 100 );
                Thread.Sleep( 5 );
                //��ȡ���ݿ��е�ǰ�û�����һ��΢����ID
                long lLastStatusIDOf = Status.GetLastStatusIDOf( lCurrentUID );

                ///@2010-10-11
                ///���ǵ���ֹ����ʱ���ܻ��ж����۵ı��棬�ʴ˴���������ȡ����һ��΢��������
                #region ��ȡ���ݿ�������һ��΢��������
                if (lLastStatusIDOf > 0)
                {
                    if (blnAsyncCancelled) return;
                    while (blnSuspending)
                    {
                        if (blnAsyncCancelled) return;
                        Thread.Sleep( 50 );
                    }

                    //��־
                    strLog = DateTime.Now.ToString() + "  " + "��ȡ΢��" + lLastStatusIDOf.ToString() + "������...";
                    bwAsync.ReportProgress( 100 );
                    Thread.Sleep( 5 );
                    //��ȡ��ǰ΢��������
                    List<Comment> lstComment = crawler.GetCommentsOf( lLastStatusIDOf );
                    //��־
                    strLog = DateTime.Now.ToString() + "  " + "����" + lstComment.Count.ToString() + "�����ۡ�";
                    bwAsync.ReportProgress( 100 );
                    Thread.Sleep( 5 );
                    if (blnAsyncCancelled) return;
                    while (blnSuspending)
                    {
                        if (blnAsyncCancelled) return;
                        Thread.Sleep( 50 );
                    }

                    foreach (Comment comment in lstComment)
                    {
                        //Thread.Sleep( 5 );
                        if (blnAsyncCancelled) return;
                        while (blnSuspending)
                        {
                            if (blnAsyncCancelled) return;
                            Thread.Sleep( 50 );
                        }
                        if (!Comment.Exists( comment.comment_id ))
                        {
                            //��־
                            strLog = DateTime.Now.ToString() + "  " + "������" + comment.comment_id.ToString() + "�������ݿ�...";
                            bwAsync.ReportProgress( 100 );
                            Thread.Sleep( 5 );
                            comment.Add();

                            //�������˼�����С���2010-10-11
                            //��־
                            strLog = DateTime.Now.ToString() + "  " + "��������" + comment.uid.ToString() + "�������...";
                            bwAsync.ReportProgress( 100 );
                            Thread.Sleep( 5 );
                            lCommentUID = comment.uid;
                            if (lstWaitingUID.Contains( lCommentUID ) || queueBuffer.Contains( lCommentUID ))
                            {
                                //��־
                                strLog = DateTime.Now.ToString() + "  " + "�û�" + lCommentUID.ToString() + "���ڶ�����...";
                                bwAsync.ReportProgress( 100 );
                            }
                            else
                            {
                                //���ڴ����Ѵﵽ���ޣ���ʹ�����ݿ���л���
                                //����ʹ�����ݿ���л���
                                if (lstWaitingUID.Count < iQueueLength)
                                    lstWaitingUID.AddLast( lCommentUID );
                                else
                                    queueBuffer.Enqueue( lCommentUID );

                                //��־
                                strLog = DateTime.Now.ToString() + "  " + "���û�" + lCommentUID.ToString() + "������С��ڴ��������" + lstWaitingUID.Count.ToString() + "���û������ݿ��������" + queueBuffer.Count.ToString() + "���û���";
                                strQueueInfo = DateTime.Now.ToString() + "  " + "΢�������˵��ڴ����������" + lstWaitingUID.Count + "���û������ݿ����������" + queueBuffer.Count.ToString() + "���û���";
                                bwAsync.ReportProgress( 100 );                                
                            }
                        }
                    }
                }
                #endregion

                if (blnAsyncCancelled) return;
                while (blnSuspending)
                {
                    if (blnAsyncCancelled) return;
                    Thread.Sleep( 50 );
                }
                //��־
                strLog = DateTime.Now.ToString() + "  " + "��ȡ�û�" + lCurrentUID.ToString() + "��ID��" + lLastStatusIDOf.ToString() + "֮���΢��...";
                bwAsync.ReportProgress( 100 );
                Thread.Sleep( 5 );
                //��ȡ���ݿ��е�ǰ�û�����һ��΢����ID֮���΢�����������ݿ�
                List<Status> lstStatus = crawler.GetStatusesOfSince( lCurrentUID, lLastStatusIDOf );
                //��־
                strLog = DateTime.Now.ToString() + "  " + "����" + lstStatus.Count.ToString() + "��΢����";
                bwAsync.ReportProgress( 100 );
                Thread.Sleep( 5 );
                #endregion
                #region ΢����Ӧ����
                foreach (Status status in lstStatus)
                {
                    if (blnAsyncCancelled) return;
                    while (blnSuspending)
                    {
                        if (blnAsyncCancelled) return;
                        Thread.Sleep( 50 );
                    }
                    if (!Status.Exists( status.status_id ))
                    {
                        //��־
                        strLog = DateTime.Now.ToString() + "  " + "��΢��" + status.status_id.ToString() + "�������ݿ�...";
                        bwAsync.ReportProgress( 100 );
                        Thread.Sleep( 5 );
                        status.Add();
                    }
                    //����΢����ת������ת��΢��ID���
                    if (status.retweeted_status_id > 0)
                    {
                        //��־
                        strLog = DateTime.Now.ToString() + "  " + "΢��" + status.status_id.ToString() + "��ת��΢������ת��΢��" + status.retweeted_status_id.ToString() + "����΢������...";
                        bwAsync.ReportProgress( 100 );
                        Thread.Sleep( 5 );
                        lstRetweetedStatus.AddLast( status.retweeted_status_id );
                    }
                    if (blnAsyncCancelled) return;
                    while (blnSuspending)
                    {
                        if (blnAsyncCancelled) return;
                        Thread.Sleep( 50 );
                    }

                    //��־
                    strLog = DateTime.Now.ToString() + "  " + "��ȡ΢��" + status.status_id.ToString() + "������...";
                    bwAsync.ReportProgress( 100 );
                    Thread.Sleep( 5 );
                    //��ȡ��ǰ΢��������
                    List<Comment> lstComment = crawler.GetCommentsOf( status.status_id );
                    //��־
                    strLog = DateTime.Now.ToString() + "  " + "����" + lstComment.Count.ToString() + "�����ۡ�";
                    bwAsync.ReportProgress( 100 );
                    Thread.Sleep( 5 );

                    foreach (Comment comment in lstComment)
                    {
                        //Thread.Sleep( 5 );
                        if (blnAsyncCancelled) return;
                        while (blnSuspending)
                        {
                            if (blnAsyncCancelled) return;
                            Thread.Sleep( 50 );
                        }
                        if (!Comment.Exists( comment.comment_id ))
                        {
                            //��־
                            strLog = DateTime.Now.ToString() + "  " + "������" + comment.comment_id.ToString() + "�������ݿ�...";
                            bwAsync.ReportProgress( 100 );
                            Thread.Sleep( 5 );
                            comment.Add();

                            //�������˼�����С���2010-10-11
                            //��־
                            strLog = DateTime.Now.ToString() + "  " + "��������" + comment.uid.ToString() + "�������...";
                            bwAsync.ReportProgress( 100 );
                            Thread.Sleep( 5 );
                            long lCommentUID = comment.uid;
                            if (lstWaitingUID.Contains( lCommentUID ) || queueBuffer.Contains( lCommentUID ))
                            {
                                //��־
                                strLog = DateTime.Now.ToString() + "  " + "�û�" + lCommentUID.ToString() + "���ڶ�����...";
                                bwAsync.ReportProgress( 100 );
                            }
                            else
                            {
                                //���ڴ����Ѵﵽ���ޣ���ʹ�����ݿ���л���
                                //����ʹ�����ݿ���л���
                                if (lstWaitingUID.Count < iQueueLength)
                                    lstWaitingUID.AddLast( lCommentUID );
                                else
                                    queueBuffer.Enqueue( lCommentUID );

                                //��־
                                strLog = DateTime.Now.ToString() + "  " + "���û�" + lCommentUID.ToString() + "������С��ڴ��������" + lstWaitingUID.Count.ToString() + "���û������ݿ��������" + queueBuffer.Count.ToString() + "���û���";
                                strQueueInfo = DateTime.Now.ToString() + "  " + "΢�������˵��ڴ����������" + lstWaitingUID.Count + "���û������ݿ����������" + queueBuffer.Count.ToString() + "���û���";
                                bwAsync.ReportProgress( 100 );                                
                            }
                            Thread.Sleep( 5 );
                        }
                    }
                }
                #endregion                
                #region ��ȡ��ȡ��ת��΢��
                while(lstRetweetedStatus.Count>0)
                {
                    if (blnAsyncCancelled) return;
                    while (blnSuspending)
                    {
                        if (blnAsyncCancelled) return;
                        Thread.Sleep( 50 );
                    }

                    long lRetweetedStatusID = lstRetweetedStatus.First.Value;
                    Status status = crawler.GetStatus( lRetweetedStatusID );
                    if(status!=null)
                    {
                        if (!Status.Exists( lRetweetedStatusID ))
                        {
                            //��־
                            strLog = DateTime.Now.ToString() + "  " + "��΢��" + lRetweetedStatusID.ToString() + "�������ݿ�...";
                            bwAsync.ReportProgress( 100 );
                            Thread.Sleep( 5 );
                            status.Add();
                        }
                        //����ת��΢����UID��ӡ���2010-10-18
                        long lRetweetUID = status.uid;
                        //��־
                        strLog = DateTime.Now.ToString() + "  " + "��΢���ķ�����" + lRetweetUID.ToString() + "�������...";
                        bwAsync.ReportProgress( 100 );
                        Thread.Sleep( 5 );

                        if (lstWaitingUID.Contains( lRetweetUID ) || queueBuffer.Contains( lRetweetUID ))
                        {
                            //��־
                            strLog = DateTime.Now.ToString() + "  " + "�û�" + lRetweetUID.ToString() + "���ڶ�����...";
                            bwAsync.ReportProgress( 100 );
                        }
                        else
                        {
                            //���ڴ����Ѵﵽ���ޣ���ʹ�����ݿ���л���
                            //����ʹ�����ݿ���л���
                            if (lstWaitingUID.Count < iQueueLength)
                                lstWaitingUID.AddLast( lRetweetUID );
                            else
                                queueBuffer.Enqueue( lRetweetUID );

                            //��־
                            strLog = DateTime.Now.ToString() + "  " + "���û�" + lRetweetUID.ToString() + "������С��ڴ��������" + lstWaitingUID.Count + "���û������ݿ��������" + queueBuffer.Count.ToString() + "���û���";
                            strQueueInfo = DateTime.Now.ToString() + "  " + "΢�������˵��ڴ����������" + lstWaitingUID.Count + "���û������ݿ����������" + queueBuffer.Count.ToString() + "���û���";
                            bwAsync.ReportProgress( 100 );                            
                        }
                        Thread.Sleep( 5 );

                        //����΢����ת������ת��΢��ID���
                        if (status.retweeted_status_id > 0)
                        {
                            //��־
                            strLog = DateTime.Now.ToString() + "  " + "΢��" + status.status_id.ToString() + "��ת��΢������ת��΢��" + status.retweeted_status_id.ToString() + "����΢������...";
                            bwAsync.ReportProgress( 100 );
                            Thread.Sleep( 5 );
                            lstRetweetedStatus.AddLast( status.retweeted_status_id );
                        }
                        if (blnAsyncCancelled) return;
                        while (blnSuspending)
                        {
                            if (blnAsyncCancelled) return;
                            Thread.Sleep( 50 );
                        }

                        //��־
                        strLog = DateTime.Now.ToString() + "  " + "��ȡ΢��" + lRetweetedStatusID.ToString() + "������...";
                        bwAsync.ReportProgress( 100 );
                        Thread.Sleep( 5 );
                        //��ȡ��ǰ΢��������
                        List<Comment> lstComment = crawler.GetCommentsOf( lRetweetedStatusID );
                        //��־
                        strLog = DateTime.Now.ToString() + "  " + "����" + lstComment.Count.ToString() + "�����ۡ�";
                        bwAsync.ReportProgress( 100 );
                        Thread.Sleep( 5 );

                        foreach (Comment comment in lstComment)
                        {
                            //Thread.Sleep( 5 );
                            if (blnAsyncCancelled) return;
                            while (blnSuspending)
                            {
                                if (blnAsyncCancelled) return;
                                Thread.Sleep( 50 );
                            }
                            if (!Comment.Exists( comment.comment_id ))
                            {
                                //��־
                                strLog = DateTime.Now.ToString() + "  " + "������" + comment.comment_id.ToString() + "�������ݿ�...";
                                bwAsync.ReportProgress( 100 );
                                Thread.Sleep( 5 );
                                comment.Add();

                                //�������˼�����С���2010-10-11
                                long lCommentUID = comment.uid;
                                //��־
                                strLog = DateTime.Now.ToString() + "  " + "��������" + lCommentUID.ToString() + "�������...";
                                bwAsync.ReportProgress( 100 );
                                Thread.Sleep( 5 );                                
                                if (lstWaitingUID.Contains( lCommentUID ) || queueBuffer.Contains( lCommentUID ))
                                {
                                    //��־
                                    strLog = DateTime.Now.ToString() + "  " + "�û�" + lCommentUID.ToString() + "���ڶ�����...";
                                    bwAsync.ReportProgress( 100 );
                                }
                                else
                                {
                                    //���ڴ����Ѵﵽ���ޣ���ʹ�����ݿ���л���
                                    //����ʹ�����ݿ���л���
                                    if (lstWaitingUID.Count < iQueueLength)
                                        lstWaitingUID.AddLast( lCommentUID );
                                    else
                                        queueBuffer.Enqueue( lCommentUID );

                                    //��־
                                    strLog = DateTime.Now.ToString() + "  " + "���û�" + lCommentUID.ToString() + "������С��ڴ��������" + lstWaitingUID.Count + "���û������ݿ��������" + queueBuffer.Count.ToString() + "���û���";
                                    strQueueInfo = DateTime.Now.ToString() + "  " + "΢�������˵��ڴ����������" + lstWaitingUID.Count + "���û������ݿ����������" + queueBuffer.Count.ToString() + "���û���";
                                    bwAsync.ReportProgress( 100 );                                    
                                }
                                Thread.Sleep( 5 );
                            }
                        }
                    }
                    lstRetweetedStatus.RemoveFirst();
                }
                #endregion
                //����ٽ��ո��������UID�����β
                //��־
                strLog = DateTime.Now.ToString() + "  " + "�û�" + lCurrentUID.ToString() + "����������ȡ��ϣ���������β...";
                bwAsync.ReportProgress( 100 );
                Thread.Sleep( 5 );
                //���ڴ����Ѵﵽ���ޣ���ʹ�����ݿ���л���
                if (lstWaitingUID.Count < iQueueLength)
                    lstWaitingUID.AddLast( lCurrentUID );
                else
                    queueBuffer.Enqueue( lCurrentUID );
                //��������Ƶ��
                //����û�����Ƶ��
                crawler.AdjustFreq();
                //��־
                strLog = DateTime.Now.ToString() + "  " + "����������Ϊ" + crawler.SleepTime.ToString() + "���롣��Сʱʣ��" + crawler.ResetTimeInSeconds.ToString() + "�룬ʣ���������Ϊ" + crawler.RemainingHits.ToString() + "��";
                bwAsync.ReportProgress( 100 );
                Thread.Sleep( 5 );
            }
        }

        public override void Initialize ()
        {
            //��ʼ����Ӧ����
            blnAsyncCancelled = false;
            blnSuspending = false;
            if (lstWaitingUID != null) lstWaitingUID.Clear();

            //������ݿ���л���
            queueBuffer.Clear();
        }
    }
}