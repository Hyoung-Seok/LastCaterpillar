using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using MCPForUnity.Editor.Services;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using Debug = UnityEngine.Debug;

// 메인 툴바(재생 버튼 줄)에 "Claude" 버튼 추가.
// - 누르면 프로젝트 루트에서 터미널을 열고 claude를 실행한다.
// - 아이콘 = MCP 세션 상태 (초록 = 세션 활성, 빨강 = 세션 없음). MCP 창의 표시와 같은 값(Bridge.IsRunning)을 본다.
// MCP 세션 자동 시작은 MCP for Unity → Advanced Settings → "Auto-Start on Editor Load".
public static class ClaudeToolbarButton
{
    private const string ElementPath = "Tools/Start Claude";
    private const double PollInterval = 0.5; // 초

    private static bool _lastIsRunning;
    private static double _nextPollTime;

    [InitializeOnLoadMethod]
    private static void RegisterPolling()
    {
        _lastIsRunning = IsMcpRunning();
        EditorApplication.update += PollMcpState;
    }

    // 상태가 바뀔 때만 툴바 재생성 (매 프레임 Refresh 하지 않음)
    private static void PollMcpState()
    {
        double now = EditorApplication.timeSinceStartup;
        if (now < _nextPollTime)
            return;
        _nextPollTime = now + PollInterval;

        bool isRunning = IsMcpRunning();
        if (isRunning == _lastIsRunning)
            return;

        _lastIsRunning = isRunning;
        MainToolbar.Refresh(ElementPath);
    }

    [MainToolbarElement(ElementPath, defaultDockPosition = MainToolbarDockPosition.Middle)]
    public static MainToolbarElement CreateButton()
    {
        bool isRunning = IsMcpRunning();

        var icon = EditorGUIUtility.IconContent(isRunning ? "greenLight" : "redLight").image as Texture2D;
        string tooltip = isRunning
            ? "MCP 세션 활성 — 클릭: 프로젝트 루트에서 Claude Code 실행"
            : "MCP 세션 없음 — 클릭: 프로젝트 루트에서 Claude Code 실행 (MCP 창에서 세션 시작 필요)";

        var content = new MainToolbarContent("Claude", icon, tooltip);
        return new MainToolbarButton(content, LaunchClaude);
    }

    private static bool IsMcpRunning()
    {
        // 패키지 쪽 예외로 툴바 전체가 깨지지 않게 방어
        try
        {
            return MCPServiceLocator.Bridge.IsRunning;
        }
        catch
        {
            return false;
        }
    }

    private static void LaunchClaude()
    {
        // Application.dataPath = <프로젝트>/Assets → 상위가 프로젝트 루트
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        // 1순위: Windows Terminal. cmd /k로 감싸서 claude 종료 후에도 창 유지
        if (TryStart("wt.exe", $"-d \"{root}\" cmd /k claude", root))
            return;

        // 2순위: 기본 cmd 창
        if (TryStart("cmd.exe", "/k claude", root))
            return;

        Debug.LogError("[ClaudeToolbarButton] 터미널을 실행하지 못했습니다 (wt.exe, cmd.exe 둘 다 실패).");
    }

    private static bool TryStart(string fileName, string arguments, string workingDirectory)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = true,
            });
            return true;
        }
        catch (Win32Exception)
        {
            // 실행 파일 없음 → 다음 후보로
            return false;
        }
    }
}
