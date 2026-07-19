import React, { useState } from 'react';
import { useGetOwnedCampaigns, useGetParticipatedCampaigns } from '../hooks/useQueries';
import { useInternetIdentity } from '../hooks/useInternetIdentity';
import { Campaign } from '../backend';
import CampaignCard from './CampaignCard';
import CreateCampaignDialog from './CreateCampaignDialog';
import CampaignDetailView from './CampaignDetailView';
import TagNavigationView from './TagNavigationView';
import { Button } from '@/components/ui/button';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Plus, Sword, BookOpen, Crown, Users } from 'lucide-react';
import { Skeleton } from '@/components/ui/skeleton';

type ViewState = 
  | { type: 'dashboard' }
  | { type: 'campaign'; campaign: Campaign }
  | { type: 'tag'; tag: string };

export default function CampaignDashboard() {
  const { data: ownedCampaigns, isLoading: ownedLoading } = useGetOwnedCampaigns();
  const { data: participatedCampaigns, isLoading: participatedLoading } = useGetParticipatedCampaigns();
  const { identity } = useInternetIdentity();
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
  const [viewState, setViewState] = useState<ViewState>({ type: 'dashboard' });

  const currentUserPrincipal = identity?.getPrincipal();

  // Filter out owned campaigns from participated campaigns to avoid duplicates
  const joinedCampaigns = participatedCampaigns?.filter(
    campaign => campaign.owner.toString() !== currentUserPrincipal?.toString()
  ) || [];

  const isLoading = ownedLoading || participatedLoading;

  const handleCampaignClick = (campaign: Campaign) => {
    setViewState({ type: 'campaign', campaign });
  };

  const handleTagClick = (tag: string) => {
    setViewState({ type: 'tag', tag });
  };

  const handleBackToDashboard = () => {
    setViewState({ type: 'dashboard' });
  };

  // Handle different view states
  if (viewState.type === 'campaign') {
    const isOwner = viewState.campaign.owner.toString() === currentUserPrincipal?.toString();
    return (
      <CampaignDetailView 
        campaign={viewState.campaign} 
        isOwner={isOwner}
        onBack={handleBackToDashboard}
      />
    );
  }

  if (viewState.type === 'tag') {
    return (
      <TagNavigationView
        selectedTag={viewState.tag}
        onBack={handleBackToDashboard}
        onNoteClick={() => {}} // This will be handled by the TagNavigationView internally
      />
    );
  }

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <Skeleton className="h-8 w-48 bg-white/10" />
          <Skeleton className="h-10 w-32 bg-white/10" />
        </div>
        <Skeleton className="h-10 w-full bg-white/10" />
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {[...Array(6)].map((_, i) => (
            <Skeleton key={i} className="h-48 bg-white/10" />
          ))}
        </div>
      </div>
    );
  }

  const totalCampaigns = (ownedCampaigns?.length || 0) + joinedCampaigns.length;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-3">
          <BookOpen className="h-8 w-8 text-purple-400" />
          <div>
            <h2 className="text-3xl font-bold text-white">Campaign Dashboard</h2>
            <p className="text-purple-200">
              {totalCampaigns === 0 
                ? "No campaigns yet. Create your first adventure!" 
                : `Managing ${totalCampaigns} campaign${totalCampaigns === 1 ? '' : 's'}`
              }
            </p>
          </div>
        </div>
        <Button
          onClick={() => setIsCreateDialogOpen(true)}
          className="bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 text-white border-0"
        >
          <Plus className="h-4 w-4 mr-2" />
          New Campaign
        </Button>
      </div>

      {totalCampaigns === 0 ? (
        <div className="text-center py-16">
          <div className="bg-white/5 backdrop-blur-sm rounded-lg p-12 border border-white/10 max-w-md mx-auto">
            <Sword className="h-16 w-16 text-purple-400 mx-auto mb-6" />
            <h3 className="text-2xl font-bold text-white mb-4">Start Your First Campaign</h3>
            <p className="text-purple-200 mb-6">
              Create your first RPG campaign and begin your adventure. Every great story starts with a single step.
            </p>
            <Button
              onClick={() => setIsCreateDialogOpen(true)}
              className="bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 text-white border-0"
            >
              <Plus className="h-4 w-4 mr-2" />
              Create Campaign
            </Button>
          </div>
        </div>
      ) : (
        <Tabs defaultValue="owned" className="w-full">
          <TabsList className="grid w-full grid-cols-2 bg-white/10">
            <TabsTrigger 
              value="owned" 
              className="data-[state=active]:bg-white/20 flex items-center gap-2"
            >
              <Crown className="h-4 w-4" />
              My Campaigns ({ownedCampaigns?.length || 0})
            </TabsTrigger>
            <TabsTrigger 
              value="joined" 
              className="data-[state=active]:bg-white/20 flex items-center gap-2"
            >
              <Users className="h-4 w-4" />
              Joined Campaigns ({joinedCampaigns.length})
            </TabsTrigger>
          </TabsList>

          <TabsContent value="owned" className="space-y-6 mt-6">
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2">
                <Crown className="h-6 w-6 text-amber-400" />
                <h3 className="text-xl font-semibold text-white">Campaigns You Own</h3>
              </div>
              <p className="text-purple-300 text-sm">
                {ownedCampaigns?.length === 0 
                  ? "You haven't created any campaigns yet"
                  : `${ownedCampaigns?.length} campaign${ownedCampaigns?.length === 1 ? '' : 's'}`
                }
              </p>
            </div>

            {ownedCampaigns?.length === 0 ? (
              <div className="text-center py-12">
                <div className="bg-white/5 backdrop-blur-sm rounded-lg p-8 border border-white/10 max-w-md mx-auto">
                  <Crown className="h-12 w-12 text-amber-400 mx-auto mb-4" />
                  <h4 className="text-lg font-semibold text-white mb-2">No Owned Campaigns</h4>
                  <p className="text-purple-200 mb-4 text-sm">
                    Create your first campaign to start managing your own RPG adventures.
                  </p>
                  <Button
                    onClick={() => setIsCreateDialogOpen(true)}
                    size="sm"
                    className="bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 text-white border-0"
                  >
                    <Plus className="h-4 w-4 mr-2" />
                    Create Campaign
                  </Button>
                </div>
              </div>
            ) : (
              <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
                {ownedCampaigns?.map((campaign) => (
                  <CampaignCard 
                    key={campaign.id} 
                    campaign={campaign} 
                    isOwner={true}
                    onCampaignClick={handleCampaignClick}
                  />
                ))}
              </div>
            )}
          </TabsContent>

          <TabsContent value="joined" className="space-y-6 mt-6">
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2">
                <Users className="h-6 w-6 text-blue-400" />
                <h3 className="text-xl font-semibold text-white">Campaigns You've Joined</h3>
              </div>
              <p className="text-purple-300 text-sm">
                {joinedCampaigns.length === 0 
                  ? "You haven't joined any campaigns yet"
                  : `${joinedCampaigns.length} campaign${joinedCampaigns.length === 1 ? '' : 's'}`
                }
              </p>
            </div>

            {joinedCampaigns.length === 0 ? (
              <div className="text-center py-12">
                <div className="bg-white/5 backdrop-blur-sm rounded-lg p-8 border border-white/10 max-w-md mx-auto">
                  <Users className="h-12 w-12 text-blue-400 mx-auto mb-4" />
                  <h4 className="text-lg font-semibold text-white mb-2">No Joined Campaigns</h4>
                  <p className="text-purple-200 text-sm">
                    You'll see campaigns here when you accept invitations from other players.
                  </p>
                </div>
              </div>
            ) : (
              <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
                {joinedCampaigns.map((campaign) => (
                  <CampaignCard 
                    key={campaign.id} 
                    campaign={campaign} 
                    isOwner={false}
                    onCampaignClick={handleCampaignClick}
                  />
                ))}
              </div>
            )}
          </TabsContent>
        </Tabs>
      )}

      <CreateCampaignDialog
        open={isCreateDialogOpen}
        onOpenChange={setIsCreateDialogOpen}
      />
    </div>
  );
}
