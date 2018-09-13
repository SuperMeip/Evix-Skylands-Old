using System.Collections;
using UnityEngine;

/// <summary>
///  job to be executed in another thread
/// </summary>
public class ThreadedJob {
  private bool m_IsDone = false;
  private object m_Handle = new object();
  protected System.Threading.Thread m_Thread = null;
  public bool IsDone {
    get {
      bool tmp;
      lock (m_Handle) {
        tmp = m_IsDone;
      }
      return tmp;
    }
    set {
      lock (m_Handle) {
        m_IsDone = value;
      }
    }
  }

  public virtual void Start() {
    m_Thread = new System.Threading.Thread(Run);
    m_Thread.Start();
  }
  public virtual void Abort() {
    m_Thread.Abort();
  }

  /// <summary>
  /// The function to execute in another thread, you can't have unity stuff here.
  /// </summary>
  protected virtual void ThreadFunction() { }

  /// <summary>
  /// What to do on finished, you can have unity stuff here
  /// </summary>
  protected virtual void OnFinished() { }

  public virtual bool Update() {
    if (IsDone) {
      OnFinished();
      return true;
    }
    return false;
  }

  public IEnumerator WaitFor() {
    while (!Update()) {
      yield return null;
    }
  }

  private void Run() {
    ThreadFunction();
    IsDone = true;
  }
}