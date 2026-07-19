import React, { useState } from 'react';
import { useInternetIdentity } from './hooks/useInternetIdentity';
import { useGetCallerUserProfile, useGetReceivedInvitations } from './hooks/useQueries';
import LoginButton from './components/LoginButton';
import ProfileSetup from './components/ProfileSetup';
import CampaignDashboard from './components/CampaignDashboard';
import InvitationsDialog from './components/InvitationsDialog';
import { Toaster } from '@/components/ui/sonner';
import { ThemeProvider } from 'next-themes';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Sword, Shield, Sparkles, Mail } from 'lucide-react';
import { InvitationStatus } from './backend';

export default function App() {
  const { identity, isInitializing } = useInternetIdentity();
  const { data: userProfile, isLoading: profileLoading, isFetched } = useGetCallerUserProfile();
  const { data: receivedInvitations = [] } = useGetReceivedInvitations();
  const [isInvitationsDialogOpen, setIsInvitationsDialogOpen] = useState(false);

  const isAuthenticated = !!identity;
  const showProfileSetup = isAuthenticated && !profileLoading && isFetched && userProfile === null;
  const pendingInvitations = receivedInvitations.filter(inv => inv.status === InvitationStatus.pending);

  if (isInitializing) {
    return (
      <ThemeProvider attribute="class" defaultTheme="dark" enableSystem>
        <div className="min-h-screen bg-gradient-to-br from-purple-900 via-blue-900 to-indigo-900 flex items-center justify-center">
          <div className="text-center space-y-4">
            <div className="flex items-center justify-center space-x-2 mb-4">
              <Sword className="h-8 w-8 text-amber-400 animate-pulse" />
              <Sparkles className="h-6 w-6 text-purple-400 animate-bounce" />
              <Shield className="h-8 w-8 text-blue-400 animate-pulse" />
            </div>
            <h1 className="text-4xl font-bold text-white">CampaignManager</h1>
            <p className="text-purple-200">Initializing your adventure...</p>
          </div>
        </div>
        <Toaster />
      </ThemeProvider>
    );
  }

  return (
    <ThemeProvider attribute="class" defaultTheme="dark" enableSystem>
      <div className="min-h-screen bg-gradient-to-br from-purple-900 via-blue-900 to-indigo-900">
        <header className="border-b border-white/10 bg-black/20 backdrop-blur-sm">
          <div className="container mx-auto px-4 py-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-3">
                <div className="flex items-center space-x-2">
                  <Sword className="h-8 w-8 text-amber-400" />
                  <Sparkles className="h-6 w-6 text-purple-400" />
                  <Shield className="h-8 w-8 text-blue-400" />
                </div>
                <div>
                  <h1 className="text-2xl font-bold text-white">CampaignManager</h1>
                  <p className="text-sm text-purple-200">Manage your RPG adventures</p>
                </div>
              </div>
              <div className="flex items-center space-x-4">
                {isAuthenticated && (
                  <Button
                    onClick={() => setIsInvitationsDialogOpen(true)}
                    variant="outline"
                    className="border-white/20 text-white hover:bg-white/10 relative"
                  >
                    <Mail className="h-4 w-4 mr-2" />
                    Invitations
                    {pendingInvitations.length > 0 && (
                      <Badge 
                        variant="destructive" 
                        className="absolute -top-2 -right-2 h-5 w-5 p-0 flex items-center justify-center text-xs"
                      >
                        {pendingInvitations.length}
                      </Badge>
                    )}
                  </Button>
                )}
                {isAuthenticated && userProfile && (
                  <div className="text-right">
                    <p className="text-sm text-purple-200">Welcome back,</p>
                    <p className="font-semibold text-white">{userProfile.name}</p>
                  </div>
                )}
                <LoginButton />
              </div>
            </div>
          </div>
        </header>

        <main className="container mx-auto px-4 py-8">
          {!isAuthenticated ? (
            <div className="text-center space-y-8 max-w-2xl mx-auto">
              <div className="space-y-4">
                <div className="flex items-center justify-center space-x-3 mb-6">
                  <Sword className="h-16 w-16 text-amber-400" />
                  <Sparkles className="h-12 w-12 text-purple-400" />
                  <Shield className="h-16 w-16 text-blue-400" />
                </div>
                <h2 className="text-4xl font-bold text-white mb-4">
                  Welcome to CampaignManager
                </h2>
                <p className="text-xl text-purple-200 mb-8">
                  Your ultimate tool for managing RPG campaigns. Create, organize, and track your adventures with ease.
                </p>
              </div>
              
              <div className="grid md:grid-cols-3 gap-6 mb-8">
                <div className="bg-white/10 backdrop-blur-sm rounded-lg p-6 border border-white/20">
                  <Sword className="h-12 w-12 text-amber-400 mx-auto mb-4" />
                  <h3 className="text-lg font-semibold text-white mb-2">Create Campaigns</h3>
                  <p className="text-purple-200 text-sm">Design and organize your RPG campaigns with detailed descriptions and settings.</p>
                </div>
                <div className="bg-white/10 backdrop-blur-sm rounded-lg p-6 border border-white/20">
                  <Shield className="h-12 w-12 text-blue-400 mx-auto mb-4" />
                  <h3 className="text-lg font-semibold text-white mb-2">Secure Management</h3>
                  <p className="text-purple-200 text-sm">Your campaigns are securely stored and only accessible by you.</p>
                </div>
                <div className="bg-white/10 backdrop-blur-sm rounded-lg p-6 border border-white/20">
                  <Sparkles className="h-12 w-12 text-purple-400 mx-auto mb-4" />
                  <h3 className="text-lg font-semibold text-white mb-2">Easy Access</h3>
                  <p className="text-purple-200 text-sm">Access your campaigns from anywhere with Internet Identity authentication.</p>
                </div>
              </div>

              <div className="bg-white/5 backdrop-blur-sm rounded-lg p-8 border border-white/10">
                <h3 className="text-2xl font-bold text-white mb-4">Ready to Begin Your Adventure?</h3>
                <p className="text-purple-200 mb-6">
                  Sign in with Internet Identity to start creating and managing your RPG campaigns.
                </p>
                <LoginButton />
              </div>
            </div>
          ) : showProfileSetup ? (
            <ProfileSetup />
          ) : (
            <CampaignDashboard />
          )}
        </main>

        <footer className="border-t border-white/10 bg-black/20 backdrop-blur-sm mt-16">
          <div className="container mx-auto px-4 py-6">
            <div className="text-center text-purple-200">
              <p>© 2025. Built with ❤️ using <a href="https://caffeine.ai" className="text-purple-400 hover:text-purple-300 transition-colors">caffeine.ai</a></p>
            </div>
          </div>
        </footer>

        <InvitationsDialog
          open={isInvitationsDialogOpen}
          onOpenChange={setIsInvitationsDialogOpen}
        />
      </div>
      <Toaster />
    </ThemeProvider>
  );
}
