<script lang="ts">
  import { Swords } from "@lucide/svelte";
  import { Heading } from "$lib/components/ui/typography";
</script>

<div class="mb-8 text-center">
  <div class="mb-6 flex justify-center">
    <Swords class="h-20 w-20 text-amber-500" aria-hidden="true" />
  </div>
  <Heading level={1} variant="display" class="mb-3 text-3xl">Welcome to Erenshor Logs</Heading>
  <p class="text-stone-400">Choose how you'd like to get started</p>
</div>
