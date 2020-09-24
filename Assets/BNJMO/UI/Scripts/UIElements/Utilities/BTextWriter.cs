using System.Collections;
using System.Collections.Generic;
using BNJMO;
using UnityEngine;

public class BTextWriter : BBehaviour
{
	[SerializeField] private float characterTypeDelay = 0.125f;
	[SerializeField] private float randomVariation = 0.05f;
	[SerializeField] private float leaveSpaceDelay = 0.22f;
	[SerializeField] private float startDelay = 0.5f;

	private BText bText;
	private string originalText;
	private IEnumerator WriteTextEnumerator;

	protected override void OnValidate()
	{
		base.OnValidate(); 

		if (CanValidate() == false)
		{
			return;
		}

		if (randomVariation >= characterTypeDelay)
		{
			randomVariation = characterTypeDelay / 10.0f;
		}
	}

	protected override void InitializeComponents()
	{
		base.InitializeComponents();

		bText = GetComponent<BText>();
		if (IS_NOT_NULL(bText))
		{
			originalText = bText.Text;
			bText.SetText("");
			bText.BUIElementEnabled += On_BText_BUIElementEnabled;
			bText.BUIElementDisabled += On_BText_BUIElementDisabled;
		}
	}

	private void On_BText_BUIElementEnabled(BUIElement obj)
	{
		StartNewCoroutine(ref WriteTextEnumerator, WriteTextCoroutine());
	}

	private void On_BText_BUIElementDisabled(BUIElement obj)
	{
		StopCoroutineIfRunning(WriteTextEnumerator);
	}


	private IEnumerator WriteTextCoroutine()
	{
		bText.SetText("");

		yield return new WaitForSeconds(startDelay);

		foreach (char c in originalText)
		{
			if (c == '\n')
			{
				yield return new WaitForSeconds(leaveSpaceDelay);
			}

			bText.SetText(bText.Text + c);

			yield return new WaitForSeconds(characterTypeDelay + Random.Range(-randomVariation, randomVariation));
		}
	}

}