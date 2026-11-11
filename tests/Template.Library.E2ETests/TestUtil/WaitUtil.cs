// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.E2ETests.TestUtil;

public static class WaitUtil
{
    public static Task WaitAsync(Func<Task<bool>> action, int maxDurationMs = 8000, int sleepMs = 1000)
    {
        return WaitAsync(action, TimeSpan.FromMilliseconds(maxDurationMs), TimeSpan.FromMilliseconds(sleepMs));
    }

    public static async Task WaitAsync(Func<Task<bool>> action, TimeSpan maxDuration, TimeSpan sleep)
    {
        if (await action())
        {
            return;
        }

        DateTime started = DateTime.UtcNow;

        do
        {
            await Task.Delay(sleep);

            if (await action())
            {
                return;
            }
        } while (DateTime.UtcNow - started <= maxDuration);
    }
}
