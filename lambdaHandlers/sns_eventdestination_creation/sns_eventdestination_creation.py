import boto3
import json
import crhelper
from crhelper import CfnResource
import cfnresponse

helper = CfnResource()

client = boto3.client('ses')

@helper.create
def onCreate(event, context):
    properties = event.get("ResourceProperties", {})
    physical_resource_id = properties.get('PhysicalResourceId', '')
    env = event['ResourceProperties']['env']
    response = client.create_configuration_set_event_destination(
        ConfigurationSetName='rxgt-atlas-connectionbox-email-configurationset-'+ env,
        EventDestination={
            'Name': 'ConnectionboxEmailEventsDestination',
            'Enabled': True,
            'MatchingEventTypes': [
                'open', 'click'
            ],
            'SNSDestination': {
                'TopicARN': event['ResourceProperties']['snsTopicArn']
            }
        }
    )
    cfnresponse.send(event, context, cfnresponse.SUCCESS, response, physical_resource_id)

@helper.update
def onUpdate(event, context):
    properties = event.get("ResourceProperties", {})
    physical_resource_id = properties.get('PhysicalResourceId', '')
    env = event['ResourceProperties']['env']
    response = client.update_configuration_set_event_destination(
        ConfigurationSetName='rxgt-atlas-connectionbox-email-configurationset-'+ env,
        EventDestination={
            'Name': 'ConnectionboxEmailEventsDestination',
            'Enabled': True,
            'MatchingEventTypes': [
                'open', 'click'
            ],
            'SNSDestination': {
                'TopicARN': event['ResourceProperties']['snsTopicArn']
            }
        }
    )
    cfnresponse.send(event, context, cfnresponse.SUCCESS, response, physical_resource_id)

@helper.delete
def onDelete(event, context):
    properties = event.get("ResourceProperties", {})
    physical_resource_id = properties.get('PhysicalResourceId', '')
    env = event['ResourceProperties']['env']
    response = client.delete_configuration_set_event_destination(
        ConfigurationSetName='rxgt-atlas-connectionbox-email-configurationset-'+ env,
        EventDestinationName='ConnectionboxEmailEventsDestination'
    )
    cfnresponse.send(event, context, cfnresponse.SUCCESS, response, physical_resource_id)

def handler(event, context):
    helper(event, context)


