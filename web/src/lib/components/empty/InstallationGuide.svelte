<!-- Installation Guide Section -->
<div id="installation-guide" class="mt-12 rounded-xl border border-stone-700 bg-stone-800 p-8">
  <h3 class="mb-6 text-2xl font-bold text-white">Installation Guide</h3>

  <div class="space-y-6">
    <!-- Step 1: BepInEx -->
    <div>
      <h4 class="mb-2 flex items-center text-lg font-semibold text-white">
        <span
          class="mr-3 flex h-6 w-6 items-center justify-center rounded-full bg-amber-500/20 text-sm font-bold text-amber-500"
        >
          1
        </span>
        Install BepInEx
      </h4>
      <div class="ml-9 space-y-2 text-sm text-stone-300">
        <p>
          Download and install
          <a
            href="https://github.com/BepInEx/BepInEx/releases"
            class="text-amber-400 underline hover:text-amber-300"
          >
            BepInEx 5.4
          </a>
          to your Erenshor game directory. BepInEx is a mod loader for Unity games.
        </p>
        <p class="text-stone-400">
          Extract the BepInEx archive to your Erenshor game folder. Your directory structure should
          look like:
        </p>
        <pre
          class="mt-2 overflow-x-auto rounded bg-stone-900 px-3 py-2 text-xs text-stone-200"><code
            >Erenshor/
├── BepInEx/
│   ├── config/
│   ├── plugins/
│   ├── patchers/
│   ├── core/
│   └── ...
├── doorstop_version
├── changelog.txt
├── doorstop_config.ini
├── winhttp.dll
├── Erenshor.exe
└── ...</code
          ></pre>
      </div>
    </div>

    <!-- Step 2: Download Mod -->
    <div>
      <h4 class="mb-2 flex items-center text-lg font-semibold text-white">
        <span
          class="mr-3 flex h-6 w-6 items-center justify-center rounded-full bg-amber-500/20 text-sm font-bold text-amber-500"
        >
          2
        </span>
        Download & Install Mod
      </h4>
      <div class="ml-9 space-y-2 text-sm text-stone-300">
        <p>Use the Download button above to get the ErenshorLogs.dll file.</p>
        <p>Place the DLL in your BepInEx plugins folder:</p>
        <p class="overflow-x-auto rounded bg-stone-900 px-3 py-2 font-mono text-xs text-stone-200">
          Erenshor/BepInEx/plugins/ErenshorLogs.dll
        </p>
      </div>
    </div>

    <!-- Step 3: Configure BepInEx -->
    <div>
      <h4 class="mb-2 flex items-center text-lg font-semibold text-white">
        <span
          class="mr-3 flex h-6 w-6 items-center justify-center rounded-full bg-amber-500/20 text-sm font-bold text-amber-500"
        >
          3
        </span>
        Configure BepInEx
      </h4>
      <div class="ml-9 space-y-3 text-sm text-stone-300">
        <p>Open your BepInEx configuration file:</p>
        <p class="overflow-x-auto rounded bg-stone-900 px-3 py-2 font-mono text-xs text-stone-200">
          Erenshor/BepInEx/config/BepInEx.cfg
        </p>

        <p class="text-stone-400">
          Find the <span class="font-semibold text-white">[Chainloader]</span> section and change:
        </p>
        <pre class="overflow-x-auto rounded bg-stone-900 px-3 py-2 text-xs text-stone-200"><code
            class="text-red-400">HideManagerGameObject = false</code
          ></pre>
        <p class="text-stone-400">To:</p>
        <pre class="overflow-x-auto rounded bg-stone-900 px-3 py-2 text-xs text-stone-200"><code
            class="text-green-400">HideManagerGameObject = true</code
          ></pre>

        <p class="mt-4 text-stone-400">
          Then find the <span class="font-semibold text-white">[Logging.Console]</span> section and change:
        </p>
        <pre class="overflow-x-auto rounded bg-stone-900 px-3 py-2 text-xs text-stone-200"><code
            class="text-red-400">Enabled = false</code
          ></pre>
        <p class="text-stone-400">To:</p>
        <pre class="overflow-x-auto rounded bg-stone-900 px-3 py-2 text-xs text-stone-200"><code
            class="text-green-400">Enabled = true</code
          ></pre>
      </div>
    </div>

    <!-- Step 4: Launch Game -->
    <div>
      <h4 class="mb-2 flex items-center text-lg font-semibold text-white">
        <span
          class="mr-3 flex h-6 w-6 items-center justify-center rounded-full bg-amber-500/20 text-sm font-bold text-amber-500"
        >
          4
        </span>
        Launch the Game
      </h4>
      <div class="ml-9 space-y-2 text-sm text-stone-300">
        <p>Start Erenshor. The mod will load automatically with BepInEx on game launch.</p>
        <p class="text-stone-400">
          You should see the mod initializing in the BepInEx console window.
        </p>
      </div>
    </div>

    <!-- Step 5: Open Web App -->
    <div>
      <h4 class="mb-2 flex items-center text-lg font-semibold text-white">
        <span
          class="mr-3 flex h-6 w-6 items-center justify-center rounded-full bg-amber-500/20 text-sm font-bold text-amber-500"
        >
          5
        </span>
        View Combat Data
      </h4>
      <div class="ml-9 space-y-2 text-sm text-stone-300">
        <p>
          Open this web app in your browser while the game is running. Enter combat in the game, and
          you'll see real-time combat events appear in the web interface.
        </p>
        <p class="text-stone-400">
          The mod streams combat data via WebSocket connection on port 38729. Data appears
          automatically when combat starts.
        </p>
      </div>
    </div>
  </div>

  <!-- Troubleshooting -->
  <div class="mt-8 border-t border-stone-700 pt-6">
    <h4 class="mb-3 text-lg font-semibold text-white">Troubleshooting</h4>
    <div class="space-y-3 text-sm text-stone-300">
      <div>
        <p class="mb-1 font-semibold text-white">Mod not loading</p>
        <p class="text-stone-400">
          Check the BepInEx console window (should appear when the game launches). Look for any
          error messages related to ErenshorLogs.
        </p>
      </div>
      <div>
        <p class="mb-1 font-semibold text-white">No combat data appearing</p>
        <p class="text-stone-400">
          Ensure the mod loaded successfully in the BepInEx console. Check your browser's developer
          console (F12) for WebSocket connection errors on port 38729.
        </p>
      </div>
      <div>
        <p class="mb-1 font-semibold text-white">Need more help?</p>
        <p class="text-stone-400">
          See the
          <a
            href="https://steamcommunity.com/sharedfiles/filedetails/?id=3485536525"
            class="text-amber-400 underline hover:text-amber-300"
          >
            Erenshor Modding guide on Steam
          </a>
          for comprehensive setup instructions and additional troubleshooting.
        </p>
      </div>
    </div>
  </div>
</div>
