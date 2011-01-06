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
    class StatusRobot : RobotBase
    {
        private UserQueue queueUserForUserInfoRobot;        //�û���Ϣ������ʹ�õ��û���������
        private UserQueue queueUserForTagRobot;             //��ǩ������ʹ�õ��û���������
        private UserQueue queueUserForUserRelationRobot;    //�û���ϵ������ʹ�õ��û���������
        private UserQueue queueUserForStatusRobot;          //΢��������ʹ�õ��û���������
        private StatusQueue queueStatus;        //΢����������

        //���캯������Ҫ������Ӧ������΢��API��������
        public StatusRobot ( SinaApiService oAPI, UserQueue qUserForUserInfoRobot, UserQueue qUserForTagRobot, UserQueue qUserForUserRelationRobot, UserQueue qUserForStatusRobot, StatusQueue qStatus )
            : base( oAPI,false )
        {
            strLogFile = Application.StartupPath + "\\" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + "_status.log";
            queueUserForUserInfoRobot = qUserForUserInfoRobot;
            queueUserForTagRobot = qUserForTagRobot;
            queueUserForUserRelationRobot = qUserForUserRelationRobot;
            queueUserForStatusRobot = qUserForStatusRobot;
            queueStatus = qStatus;
        }

        /// <summary>
        /// ������������ȡ��΢������
        /// </summary>
        private void SaveStatus ( Status status )
        {
            lCurrentID = status.status_id;
            if (!Status.Exists( lCurrentID ))
            {
                //��־
                Log( "��΢��" + lCurrentID.ToString() + "�������ݿ�..." );
                status.Add();
            }

            if (queueStatus.Enqueue( lCurrentID ))
                Log( "��΢��" + lCurrentID.ToString() + "����΢�����С�" );
            else
                Log( "΢��" + lCurrentID.ToString() + "����΢�������С�" );

            //����΢����ת������ת��΢������
            if (status.retweeted_status != null)
            {
                if (blnAsyncCancelled) return;
                while (blnSuspending)
                {
                    if (blnAsyncCancelled) return;
                    Thread.Sleep( 10 );
                }

                //��־
                Log( "΢��" + lCurrentID.ToString() + "��ת��΢������ת��΢��" + status.retweeted_status.status_id.ToString() + "�������ݿ�..." );

                if (!Status.Exists( status.retweeted_status.status_id ))
                {
                    status.retweeted_status.Add();

                    //��־
                    Log( "ת��΢��" + status.retweeted_status.status_id.ToString() + "�ѱ��档" );
                }
                else
                {
                    //��־
                    Log( "ת��΢��" + status.retweeted_status.status_id.ToString() + "�Ѵ��ڡ�" );
                }

                if (queueStatus.Enqueue( status.retweeted_status.status_id ))
                    Log( "��ת��΢��" + status.retweeted_status.status_id.ToString() + "����΢�����С�" );
                else
                    Log( "ת��΢��" + status.retweeted_status.status_id.ToString() + "����΢�������С�" );

                if (queueUserForUserInfoRobot.Enqueue( status.retweeted_status.user_id ))
                    Log( "���û�" + status.retweeted_status.user_id.ToString() + "�����û���Ϣ�����˵��û����С�" );
                else
                    Log( "�û�" + status.retweeted_status.user_id.ToString() + "�����û���Ϣ�����˵��û������С�" );
                if (queueUserForTagRobot.Enqueue( status.retweeted_status.user_id ))
                    Log( "���û�" + status.retweeted_status.user_id.ToString() + "�����ǩ�����˵��û����С�" );
                else
                    Log( "�û�" + status.retweeted_status.user_id.ToString() + "���ڱ�ǩ�����˵��û������С�" );
                if (queueUserForUserRelationRobot.Enqueue( status.retweeted_status.user_id ))
                    Log( "���û�" + status.retweeted_status.user_id.ToString() + "�����û���ϵ�����˵��û����С�" );
                else
                    Log( "�û�" + status.retweeted_status.user_id.ToString() + "�����û���ϵ�����˵��û������С�" );

                if (queueUserForStatusRobot.Enqueue( status.retweeted_status.user_id ))
                    Log( "���û�" + status.retweeted_status.user_id.ToString() + "����΢�������˵��û����С�" );
                else
                    Log( "�û�" + status.retweeted_status.user_id.ToString() + "����΢�������˵��û������С�" );
            }
        }

        /// <summary>
        /// ��ʼ��ȡ΢������
        /// </summary>
        public void Start ()
        {
            while (queueUserForStatusRobot.Count == 0)
            {
                if (blnAsyncCancelled) return;
                Thread.Sleep( 50 );   //������Ϊ�գ���ȴ�
            }
            long lStartUserID = queueUserForStatusRobot.FirstValue;
            long lCurrentUserID = 0;
            //�Զ�������ѭ�����У�ֱ���в�����ͣ��ֹͣ
            while (true)
            {
                if (blnAsyncCancelled) return;
                while (blnSuspending)
                {
                    if (blnAsyncCancelled) return;
                    Thread.Sleep( 50 );
                }

                //����ͷȡ��
                lCurrentUserID = queueUserForStatusRobot.RollQueue();

                #region Ԥ����
                if (lCurrentUserID == lStartUserID)  //˵������һ��ѭ������
                {
                    if (blnAsyncCancelled) return;
                    while (blnSuspending)
                    {
                        if (blnAsyncCancelled) return;
                        Thread.Sleep( 50 );
                    }

                    //��־
                    Log( "��ʼ����֮ǰ���ӵ�������..." );

                    Status.NewIterate();
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
                Log( "��ȡ���ݿ����û�" + lCurrentUserID.ToString() + "����һ��΢����ID..." );
                //��ȡ���ݿ��е�ǰ�û�����һ��΢����ID
                lCurrentID = Status.GetLastStatusIDOf( lCurrentUserID );

                if (blnAsyncCancelled) return;
                while (blnSuspending)
                {
                    if (blnAsyncCancelled) return;
                    Thread.Sleep( 50 );
                }

                Status status;
                #region ΢����ҳ���ϵ�ID����ʵ�ʵ�ID����ʱ����
                //if(lCurrentID==0)   //ͨ��webץȡ����΢��
                //{
                //    //��־
                //    Log( "��ȡ�û�" + lCurrentUserID.ToString()+"�Ļ�����Ϣ..." );
                //    User user = crawler.GetUserInfo( lCurrentUserID );  //��Ҫ��Ϊ�˻�ȡ�û���΢���������Թ��ο�
                //    //��־
                //    Log( "��ȡ�û�" + lCurrentUserID.ToString() + "������΢��ID�б�..." );
                //    LinkedList<long> lstStatusID=crawler.GetStatusesByWeb(lCurrentUserID,user.statuses_count);
                //    //��־
                //    Log( "����" + lstStatusID.Count.ToString() + "��΢����" );

                //    long lStatusID = 0;
                //    while(lstStatusID.Count>0)
                //    {
                //        if (blnAsyncCancelled) return;
                //        while (blnSuspending)
                //        {
                //            if (blnAsyncCancelled) return;
                //            Thread.Sleep( 50 );
                //        }

                //        lStatusID = lstStatusID.First.Value;
                //        //��־
                //        Log( "��ȡ΢��" + lStatusID.ToString() + "������..." );
                //        status = crawler.GetStatus( lStatusID );
                //        SaveStatus( status );
                //        lstStatusID.RemoveFirst();                        
                //    }
                //}
                //else
                //{
                #endregion
                #region ����΢��
                //��־
                Log( "��ȡ�û�" + lCurrentUserID.ToString() + "��ID��" + lCurrentID.ToString() + "֮���΢��..." );
                //��ȡ���ݿ��е�ǰ�û�����һ��΢����ID֮���΢�����������ݿ�
                LinkedList<Status> lstStatus = crawler.GetStatusesOfSince( lCurrentUserID, lCurrentID );
                //��־
                Log( "����" + lstStatus.Count.ToString() + "��΢����" );

                while (lstStatus.Count > 0)
                {
                    if (blnAsyncCancelled) return;
                    while (blnSuspending)
                    {
                        if (blnAsyncCancelled) return;
                        Thread.Sleep( 50 );
                    }
                    status = lstStatus.First.Value;
                    SaveStatus( status );
                    lstStatus.RemoveFirst();
                }
                #endregion
                //}
                #endregion
                //��־
                Log( "�û�" + lCurrentUserID.ToString() + "��΢����������ȡ��ϡ�" );
                //��־
                Log( "����������Ϊ" + crawler.SleepTime.ToString() + "���롣��Сʱʣ��" + crawler.ResetTimeInSeconds.ToString() + "�룬ʣ���������Ϊ" + crawler.RemainingHits.ToString() + "��" );
            }
        }

        public override void Initialize ()
        {
            //��ʼ����Ӧ����
            blnAsyncCancelled = false;
            blnSuspending = false;
            crawler.StopCrawling = false;
            queueUserForStatusRobot.Initialize();
        }
    }
}