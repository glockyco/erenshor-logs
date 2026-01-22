<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import Button from "./Button.svelte";
  import { X, Trash2 } from "@lucide/svelte";

  const { Story } = defineMeta({
    title: "UI/Button",
    component: Button,
    tags: ["autodocs"],
    argTypes: {
      variant: {
        control: "select",
        options: ["default", "primary", "ghost", "danger"],
      },
      size: {
        control: "select",
        options: ["sm", "md", "lg"],
      },
      disabled: {
        control: "boolean",
      },
    },
  });
</script>

<Story name="Default">
  {#snippet template(_args)}
    <Button>Default Button</Button>
  {/snippet}
</Story>

<Story name="Primary">
  {#snippet template(_args)}
    <Button variant="primary">Primary Button</Button>
  {/snippet}
</Story>

<Story name="Ghost">
  {#snippet template(_args)}
    <Button variant="ghost">Ghost Button</Button>
  {/snippet}
</Story>

<Story name="Danger">
  {#snippet template(_args)}
    <Button variant="danger">Danger Button</Button>
  {/snippet}
</Story>

<Story name="Sizes">
  {#snippet template(_args)}
    <div class="flex items-center gap-4">
      <Button size="sm">Small</Button>
      <Button size="md">Medium</Button>
      <Button size="lg">Large</Button>
    </div>
  {/snippet}
</Story>

<Story name="Primary Sizes">
  {#snippet template(_args)}
    <div class="flex items-center gap-4">
      <Button variant="primary" size="sm">Small</Button>
      <Button variant="primary" size="md">Medium</Button>
      <Button variant="primary" size="lg">Large</Button>
    </div>
  {/snippet}
</Story>

<Story name="Disabled">
  {#snippet template(_args)}
    <div class="flex items-center gap-4">
      <Button disabled>Disabled Default</Button>
      <Button variant="primary" disabled>Disabled Primary</Button>
    </div>
  {/snippet}
</Story>

<Story name="All Variants">
  {#snippet template(_args)}
    <div class="flex flex-wrap gap-4">
      <Button variant="default">Default</Button>
      <Button variant="primary">Primary</Button>
      <Button variant="ghost">Ghost</Button>
      <Button variant="danger">Danger</Button>
    </div>
  {/snippet}
</Story>

<Story name="Submit Button">
  {#snippet template(_args)}
    <form onsubmit={(e) => e.preventDefault()}>
      <div class="flex items-center gap-4">
        <Button type="button" variant="ghost">Cancel</Button>
        <Button type="submit" variant="primary">Submit Form</Button>
      </div>
    </form>
  {/snippet}
</Story>

<Story name="With ARIA Label">
  {#snippet template(_args)}
    <div class="flex items-center gap-4">
      <Button variant="ghost" aria-label="Close dialog">
        <X class="h-4 w-4" />
      </Button>
      <Button variant="danger" aria-label="Delete item">
        <Trash2 class="h-4 w-4" />
      </Button>
    </div>
  {/snippet}
</Story>

<Story name="With Custom Classes">
  {#snippet template(_args)}
    <div class="space-y-4">
      <Button class="w-full" variant="primary">Full Width Button</Button>
      <Button class="w-32" variant="default">Fixed Width</Button>
    </div>
  {/snippet}
</Story>
